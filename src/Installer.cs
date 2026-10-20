using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

internal static class InstallerProgram
{
    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using (StartupNotice notice = new StartupNotice())
        {
            if (notice.ShowDialog() != DialogResult.OK) return 2;
        }
        if (args.Length > 0 && args[0] == "--quick-install") return QuickInstall(args);
        if (args.Length > 0)
        {
            try
            {
                if (args.Length == 1 && args[0] == "--help")
                { Console.WriteLine("--install <PCL.exe> [--lock-updates | --no-update-lock]\n--restore <PCL.exe>\n--inspect <PCL.exe>\n--quick-install [PCL.exe]\n默认保留更新设置。警告：--lock-updates 会阻止官方自动更新，不建议使用。"); return 0; }
                if (args.Length < 2 || args.Length > 3) throw new ArgumentException("命令格式错误，请使用 --help。");
                InstallerEngine engine = new InstallerEngine(Console.WriteLine);
                if (args[0] == "--install" && (args.Length == 2 || args[2] == "--no-update-lock" || args[2] == "--lock-updates")) Console.WriteLine(engine.Install(args[1], args.Length == 3 && args[2] == "--lock-updates"));
                else if (args[0] == "--restore" && args.Length == 2) Console.WriteLine(engine.Restore(args[1]));
                else if (args[0] == "--inspect" && args.Length == 2) Console.WriteLine(engine.Inspect(args[1]));
                else throw new ArgumentException("未知命令或参数，请使用 --help。");
                return 0;
            }
            catch (Exception ex) { Console.Error.WriteLine("失败：" + ex.Message); return 1; }
        }
        Application.Run(new InstallerForm()); return 0;
    }
    private static int QuickInstall(string[] args)
    {
        try
        {
            if (args.Length > 2) throw new ArgumentException("快速安装只接受一个可选 PCL 路径。");
            string target = args.Length == 2 ? args[1] : InstallerEngine.AutoDetect();
            if (String.IsNullOrEmpty(target)) using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "选择 PCL 2.99.0.0 后安装（不锁定官方更新）";
                dialog.Filter = "PCL 可执行文件 (*.exe)|*.exe"; dialog.CheckFileExists = true;
                if (dialog.ShowDialog() != DialogResult.OK) return 2;
                target = dialog.FileName;
            }
            string result = new InstallerEngine(null).Install(target, false);
            MessageBox.Show(result + "\r\n\r\n请手动启动 PCL。如需恢复原版，请打开助手管理界面。", "PCL 下载恢复助手（第三方）", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return 0;
        }
        catch (Exception ex) { MessageBox.Show("失败：" + ex.Message, "PCL 下载恢复助手（第三方）", MessageBoxButtons.OK, MessageBoxIcon.Error); return 1; }
    }
}

[Serializable]
public sealed class RestoreManifest
{
    public int Schema = 1;
    public string TargetPath, OriginalHash, PatchedHash, BackupFileName, Status;
    public bool LockUpdates, ConfigExisted, ConfigKeyExisted, ConfigAddedSeparator, ConfigChanged;
    public string ConfigOriginalLine, ConfigBeforeHash, ConfigAfterHash;
    public bool RestoreConfigChanged;
    public string RestoreConfigBeforeHash, RestoreConfigAfterHash;
}

internal sealed class IniLine
{
    public string Text, Ending;
    public IniLine(string text, string ending) { Text = text; Ending = ending; }
}

internal sealed class IniDocument
{
    private static readonly Regex Key = new Regex(@"^(\s*SystemSystemUpdate\s*:\s*)(.*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    public readonly List<IniLine> Lines = new List<IniLine>();
    private Encoding encoding;
    private byte[] preamble;

    public static IniDocument Decode(byte[] bytes)
    {
        IniDocument doc = new IniDocument(); int skip = 0;
        if (bytes.Length >= 4 && bytes[0] == 255 && bytes[1] == 254 && bytes[2] == 0 && bytes[3] == 0)
        { doc.encoding = new UTF32Encoding(false, true, true); skip = 4; }
        else if (bytes.Length >= 4 && bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 254 && bytes[3] == 255)
        { doc.encoding = new UTF32Encoding(true, true, true); skip = 4; }
        else if (bytes.Length >= 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191)
        { doc.encoding = new UTF8Encoding(false, true); skip = 3; }
        else if (bytes.Length >= 2 && bytes[0] == 255 && bytes[1] == 254)
        { doc.encoding = new UnicodeEncoding(false, false, true); skip = 2; }
        else if (bytes.Length >= 2 && bytes[0] == 254 && bytes[1] == 255)
        { doc.encoding = new UnicodeEncoding(true, false, true); skip = 2; }
        else doc.encoding = new UTF8Encoding(false, true);
        doc.preamble = new byte[skip]; Array.Copy(bytes, doc.preamble, skip);
        string content;
        try { content = doc.encoding.GetString(bytes, skip, bytes.Length - skip); }
        catch (DecoderFallbackException)
        {
            if (skip != 0) throw new InvalidDataException("Setup.ini 字符编码无效，未修改文件。");
            doc.encoding = Encoding.GetEncoding(Encoding.Default.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            content = doc.encoding.GetString(bytes);
        }
        byte[] roundtrip = doc.encoding.GetBytes(content);
        if (content.IndexOf('\0') >= 0) throw new InvalidDataException("Setup.ini 包含空字符或未标识的编码，未修改文件。");
        if (roundtrip.Length != bytes.Length - skip) throw new InvalidDataException("Setup.ini 无法无损解码，未修改文件。");
        for (int i = 0; i < roundtrip.Length; i++) if (roundtrip[i] != bytes[i + skip]) throw new InvalidDataException("Setup.ini 无法无损解码。");
        int start = 0;
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] != '\r' && content[i] != '\n') continue;
            int end = i;
            if (content[i] == '\r' && i + 1 < content.Length && content[i + 1] == '\n') i++;
            doc.Lines.Add(new IniLine(content.Substring(start, end - start), content.Substring(end, i - end + 1)));
            start = i + 1;
        }
        if (start < content.Length) doc.Lines.Add(new IniLine(content.Substring(start), ""));
        return doc;
    }
    public int FindKey()
    {
        int found = -1;
        for (int i = 0; i < Lines.Count; i++) if (Key.IsMatch(Lines[i].Text))
        { if (found >= 0) throw new InvalidDataException("Setup.ini 中有重复的 SystemSystemUpdate 项，请先手动消除歧义。"); found = i; }
        return found;
    }
    private string ValueAt(int index) { return Key.Match(Lines[index].Text).Groups[2].Value.Trim(); }
    public void SetLock(RestoreManifest manifest)
    {
        int index = FindKey(); manifest.ConfigKeyExisted = index >= 0;
        manifest.ConfigOriginalLine = index >= 0 ? Lines[index].Text : null;
        if (index >= 0)
        { if (ValueAt(index) != "3") Lines[index].Text = Key.Match(Lines[index].Text).Groups[1].Value + "3"; return; }
        string newline = "\r\n";
        foreach (IniLine line in Lines) if (line.Ending.Length > 0) { newline = line.Ending; break; }
        if (Lines.Count > 0 && Lines[Lines.Count - 1].Ending.Length == 0)
        { manifest.ConfigAddedSeparator = true; Lines[Lines.Count - 1].Ending = newline; }
        Lines.Add(new IniLine("SystemSystemUpdate:3", newline));
    }
    public string RestoreKey(RestoreManifest manifest)
    {
        int index = FindKey();
        if (index < 0) return "更新设置已被移除，保留当前设置。";
        if (ValueAt(index) != "3") return "更新设置在安装后被修改，保留用户当前值。";
        if (manifest.ConfigKeyExisted) Lines[index].Text = manifest.ConfigOriginalLine;
        else
        {
            bool last = index == Lines.Count - 1; Lines.RemoveAt(index);
            if (last && manifest.ConfigAddedSeparator && Lines.Count > 0) Lines[Lines.Count - 1].Ending = "";
        }
        return null;
    }
    public byte[] Encode()
    {
        StringBuilder text = new StringBuilder(); foreach (IniLine line in Lines) { text.Append(line.Text); text.Append(line.Ending); }
        byte[] content = encoding.GetBytes(text.ToString()), result = new byte[preamble.Length + encoding.GetByteCount(text.ToString())];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length); Buffer.BlockCopy(content, 0, result, preamble.Length, content.Length); return result;
    }
}

internal sealed class ConfigChange
{
    public byte[] Before, After;
    public bool Existed, Changed;
    public string Warning;
    public string BeforeHash { get { return Existed ? InstallerEngine.HashBytes(Before) : "absent"; } }
    public string AfterHash { get { return Changed ? InstallerEngine.HashBytes(After) : BeforeHash; } }
}

internal sealed class TargetLock : IDisposable
{
    private Mutex mutex;
    public TargetLock(string path)
    {
        mutex = new Mutex(false, @"Local\PCLDownloadRestore." + InstallerEngine.HashBytes(Encoding.UTF8.GetBytes(path.ToUpperInvariant())));
        bool held; try { held = mutex.WaitOne(0, false); } catch (AbandonedMutexException) { held = true; }
        if (!held) { mutex.Dispose(); mutex = null; throw new IOException("另一个恢复助手正在操作此 PCL，请等待其结束。"); }
    }
    public void Dispose() { if (mutex != null) { mutex.ReleaseMutex(); mutex.Dispose(); mutex = null; } }
}

internal sealed class InstallerEngine
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetLongPathName(string shortPath, StringBuilder longPath, uint capacity);
    private readonly Action<string> log;
    public InstallerEngine(Action<string> logger) { log = logger ?? delegate(string unused) { }; }
    private static bool Same(string a, string b) { return String.Equals(a, b, StringComparison.OrdinalIgnoreCase); }
    private static string BackupPath(string target) { return Path.Combine(Path.GetDirectoryName(target), ".pcl-download-restore"); }
    private static string ConfigPath(string target) { return Path.Combine(Path.GetDirectoryName(target), "PCL", "Setup.ini"); }
    private static string ManifestPath(string target) { return Path.Combine(BackupPath(target), "manifest-" + HashBytes(Encoding.UTF8.GetBytes(target.ToUpperInvariant())).Substring(0, 24) + ".xml"); }
    private static string Temporary(string target, string kind) { return Path.Combine(Path.GetDirectoryName(target), ".pcl-restore-" + kind + "-" + Guid.NewGuid().ToString("N") + ".tmp"); }
    public static string HashBytes(byte[] bytes)
    { using (SHA256 sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", ""); }
    internal static void RejectReparse(string path)
    {
        string current = Path.GetFullPath(path);
        while (!String.IsNullOrEmpty(current))
        {
            if (File.Exists(current) || Directory.Exists(current))
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) throw new IOException("不支持符号链接、目录联接或重解析路径：" + current);
            string parent = Path.GetDirectoryName(current); if (parent == current) break; current = parent;
        }
    }
    private static string Normalize(string input)
    {
        if (String.IsNullOrWhiteSpace(input)) throw new ArgumentException("请先选择 PCL 可执行文件。");
        string target = Path.GetFullPath(input.Trim().Trim('"'));
        if (target.StartsWith(@"\\", StringComparison.Ordinal)) throw new IOException("请在本机磁盘上安装，不支持网络共享路径。");
        if (target.IndexOf(':', 2) >= 0 || !Same(Path.GetExtension(target), ".exe")) throw new IOException("目标必须是普通的 .exe 文件。");
        RejectReparse(target); if (!File.Exists(target)) throw new FileNotFoundException("未找到 PCL 可执行文件。", target);
        StringBuilder longPath = new StringBuilder(32768);
        uint length = GetLongPathName(target, longPath, (uint)longPath.Capacity);
        if (length == 0 || length >= longPath.Capacity) throw new IOException("无法规范化目标路径，请改用本机普通路径。");
        target = longPath.ToString();
        if (Same(target, Path.GetFullPath(Process.GetCurrentProcess().MainModule.FileName))) throw new IOException("不能选择恢复助手自身，请选择 PCL.exe。");
        return target;
    }
    private static void RequireStopped(string target)
    {
        foreach (Process process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(target))) using (process)
        {
            string executable;
            try { if (process.HasExited) continue; executable = process.MainModule.FileName; }
            catch (InvalidOperationException) { continue; }
            catch (Exception ex) { throw new IOException("发现同名运行进程，但无法确认路径；请先关闭所有 PCL 再试。", ex); }
            if (Same(Path.GetFullPath(executable), target)) throw new IOException("PCL 正在运行。请自行退出 PCL 后再试；助手不会终止进程。");
        }
    }
    private static void FlushWrite(string path, byte[] bytes)
    {
        RejectReparse(path);
        using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, FileOptions.WriteThrough))
        { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
    }
    private static void FlushCopy(string source, string destination)
    {
        RejectReparse(source); RejectReparse(destination);
        using (FileStream input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (FileStream output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, FileOptions.WriteThrough))
        { input.CopyTo(output); output.Flush(true); }
    }
    private static void SafeDeleteTemporary(string path)
    { if (String.IsNullOrEmpty(path)) return; try { RejectReparse(path); if (File.Exists(path)) File.Delete(path); } catch { } }
    private static RestoreManifest LoadManifest(string target)
    {
        string file = ManifestPath(target); RejectReparse(file); if (!File.Exists(file)) return null;
        if (new FileInfo(file).Length > 65536) throw new InvalidDataException("恢复记录过大，已停止操作。");
        RestoreManifest manifest; XmlReaderSettings settings = new XmlReaderSettings(); settings.DtdProcessing = DtdProcessing.Prohibit; settings.XmlResolver = null;
        using (XmlReader reader = XmlReader.Create(file, settings)) manifest = (RestoreManifest)new XmlSerializer(typeof(RestoreManifest)).Deserialize(reader);
        if (manifest.Schema != 1 || !Same(manifest.TargetPath, target) || !Same(manifest.OriginalHash, PatchCore.SupportedHash) || manifest.BackupFileName != PatchCore.SupportedHash + ".original.exe" || String.IsNullOrEmpty(manifest.PatchedHash) || !Regex.IsMatch(manifest.PatchedHash, "^[A-Fa-f0-9]{64}$"))
            throw new InvalidDataException("恢复记录与所选文件不匹配，已停止操作。");
        if (manifest.Status != "prepared" && manifest.Status != "installed" && manifest.Status != "restoring" && manifest.Status != "restored") throw new InvalidDataException("恢复记录状态无效。");
        if (manifest.ConfigKeyExisted && (String.IsNullOrEmpty(manifest.ConfigOriginalLine) || manifest.ConfigOriginalLine.IndexOfAny(new char[] { '\r', '\n', '\0' }) >= 0 || !Regex.IsMatch(manifest.ConfigOriginalLine, @"^\s*SystemSystemUpdate\s*:.*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))) throw new InvalidDataException("恢复记录中的原始设置无效。");
        return manifest;
    }
    private static void SaveManifest(string target, RestoreManifest manifest)
    {
        string destination = ManifestPath(target), stage = Temporary(destination, "manifest"); RejectReparse(destination);
        try
        {
            using (MemoryStream buffer = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings(); settings.Encoding = new UTF8Encoding(false); settings.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(buffer, settings)) new XmlSerializer(typeof(RestoreManifest)).Serialize(writer, manifest);
                FlushWrite(stage, buffer.ToArray());
            }
            if (File.Exists(destination)) File.Replace(stage, destination, null, true); else File.Move(stage, destination);
        }
        finally { SafeDeleteTemporary(stage); }
    }
    private static void VerifyBackup(string target, RestoreManifest manifest)
    {
        string backup = Path.Combine(BackupPath(target), manifest.BackupFileName); RejectReparse(backup);
        if (!File.Exists(backup) || !Same(PatchCore.Hash(backup), manifest.OriginalHash)) throw new InvalidDataException("原版备份丢失或校验不通过，已停止操作。");
    }
    private static string CurrentConfigHash(string path) { RejectReparse(path); return File.Exists(path) ? PatchCore.Hash(path) : "absent"; }
    private static ConfigChange ReadConfig(string path)
    {
        RejectReparse(path); ConfigChange change = new ConfigChange(); change.Existed = File.Exists(path);
        if (change.Existed && new FileInfo(path).Length > 16777216) throw new InvalidDataException("Setup.ini 大于 16 MB，拒绝自动修改。");
        change.Before = change.Existed ? File.ReadAllBytes(path) : new byte[0]; change.After = change.Before; return change;
    }
    private static ConfigChange PlanInstallConfig(string target, bool lockUpdates, RestoreManifest manifest)
    {
        ConfigChange change = ReadConfig(ConfigPath(target)); manifest.ConfigExisted = change.Existed; manifest.LockUpdates = lockUpdates;
        if (lockUpdates)
        {
            IniDocument doc = IniDocument.Decode(change.Before); doc.SetLock(manifest); change.After = doc.Encode();
            change.Changed = !change.Existed || !Same(HashBytes(change.Before), HashBytes(change.After));
        }
        manifest.ConfigChanged = change.Changed; manifest.ConfigBeforeHash = change.BeforeHash; manifest.ConfigAfterHash = change.AfterHash; return change;
    }
    private static ConfigChange PlanRestoreConfig(string target, RestoreManifest manifest)
    {
        ConfigChange change = ReadConfig(ConfigPath(target));
        if (manifest.ConfigChanged && manifest.LockUpdates)
        {
            if (!change.Existed) change.Warning = "Setup.ini 已被移除，未重新创建。";
            else { IniDocument doc = IniDocument.Decode(change.Before); change.Warning = doc.RestoreKey(manifest); change.After = doc.Encode(); change.Changed = !Same(HashBytes(change.Before), HashBytes(change.After)); }
        }
        return change;
    }
    private static string CommitConfig(string target, ConfigChange change)
    {
        if (!change.Changed) return null;
        string path = ConfigPath(target), stage = Temporary(path, "config"), rollback = Temporary(path, "config-backup");
        RejectReparse(path); Directory.CreateDirectory(Path.GetDirectoryName(path)); RejectReparse(path);
        try
        {
            FlushWrite(stage, change.After);
            if (!Same(CurrentConfigHash(path), change.BeforeHash)) throw new IOException("Setup.ini 在操作期间被其他程序修改，已停止替换。");
            if (change.Existed) File.Replace(stage, path, rollback, true); else File.Move(stage, path);
            return change.Existed ? rollback : "created";
        }
        finally { SafeDeleteTemporary(stage); }
    }
    private static void RollbackConfig(string target, ConfigChange change, string rollback)
    {
        if (rollback == null) return; string path = ConfigPath(target);
        if (!Same(CurrentConfigHash(path), change.AfterHash)) throw new IOException("配置回滚受阻：Setup.ini 已被其他程序修改，未覆盖。");
        if (rollback == "created") File.Delete(path); else { RejectReparse(rollback); File.Replace(rollback, path, null, true); }
    }

    private string CompleteInterruptedInstall(string target, RestoreManifest manifest)
    {
        VerifyBackup(target, manifest); PatchCore.VerifyPatched(target);
        string current = CurrentConfigHash(ConfigPath(target));
        if (manifest.ConfigChanged && !Same(current, manifest.ConfigAfterHash))
        {
            if (!Same(current, manifest.ConfigBeforeHash)) throw new IOException("上次安装中断，且配置已发生外部修改；已停止自动覆盖。");
            RestoreManifest unused = new RestoreManifest(); ConfigChange change = PlanInstallConfig(target, manifest.LockUpdates, unused);
            if (!Same(change.AfterHash, manifest.ConfigAfterHash)) throw new InvalidDataException("配置事务校验失败。");
            string temporary = CommitConfig(target, change); if (temporary != "created") SafeDeleteTemporary(temporary);
        }
        manifest.Status = "installed"; SaveManifest(target, manifest);
        return "已完成上次中断的安装。现在可以启动 PCL。";
    }

    public string Install(string input, bool lockUpdates)
    {
        string target = Normalize(input);
        using (new TargetLock(target))
        {
            RequireStopped(target); string currentHash = PatchCore.Hash(target); RestoreManifest previous = LoadManifest(target);
            if (previous != null && Same(currentHash, previous.PatchedHash))
            {
                VerifyBackup(target, previous); PatchCore.VerifyPatched(target);
                if (previous.Status == "prepared") CompleteInterruptedInstall(target, previous);
                if (previous.Status == "installed")
                {
                    if (Same(currentHash, PatchCore.LegacyPatchedHash) || previous.LockUpdates != lockUpdates)
                    {
                        log("正在迁移旧补丁或更新选项：先完整恢复，再安装新版；若安装失败将保留已恢复的原版。");
                        Restore(target);
                        return Install(target, lockUpdates);
                    }
                    return "已启用，无需重复安装。备份和当前设置均已保留。";
                }
                throw new IOException("恢复事务尚未结束，请先执行“恢复原版”。");
            }
            if (!Same(currentHash, PatchCore.SupportedHash)) throw new InvalidDataException("版本校验不通过。仅支持已核验 SHA-256 的 PCL 2.99.0.0 原版；未修改目标文件。");
            if (previous != null && previous.Status == "installed")
            {
                log("检测到官方原版已覆盖补丁；先整理已记录的更新设置，再按用户要求重新启用。");
                Restore(target); previous = LoadManifest(target);
            }
            if (previous != null && previous.Status != "restored" && previous.Status != "prepared") throw new IOException("可执行文件和恢复记录状态不一致，请检查是否手动替换过文件。");
            if (previous != null && previous.Status == "prepared" && previous.ConfigChanged && !Same(CurrentConfigHash(ConfigPath(target)), previous.ConfigBeforeHash)) throw new IOException("上次安装未完成且配置已改变，已停止自动覆盖。");
            RejectReparse(BackupPath(target)); Directory.CreateDirectory(BackupPath(target)); RejectReparse(BackupPath(target));
            RestoreManifest manifest = new RestoreManifest();
            manifest.TargetPath = target; manifest.OriginalHash = PatchCore.SupportedHash; manifest.BackupFileName = PatchCore.SupportedHash + ".original.exe";
            string backup = Path.Combine(BackupPath(target), manifest.BackupFileName);
            if (!File.Exists(backup))
            {
                log("正在写入原版备份……"); string stagedBackup = Temporary(backup, "backup");
                try
                {
                    FlushCopy(target, stagedBackup);
                    if (!Same(PatchCore.Hash(stagedBackup), manifest.OriginalHash)) throw new IOException("原版备份暂存文件校验失败。");
                    RejectReparse(backup); File.Move(stagedBackup, backup);
                }
                finally { SafeDeleteTemporary(stagedBackup); }
            }
            VerifyBackup(target, manifest);
            ConfigChange config = PlanInstallConfig(target, lockUpdates, manifest);
            string stage = Temporary(target, "patched"), rollbackExe = Temporary(target, "exe-backup"), rollbackConfig = null;
            bool replaced = false, completed = false;
            try
            {
                log("正在核验并修复下载入口……"); PatchCore.Patch(target, stage); PatchCore.VerifyPatched(stage);
                using (FileStream sync = new FileStream(stage, FileMode.Open, FileAccess.Write, FileShare.None)) sync.Flush(true);
                manifest.PatchedHash = PatchCore.Hash(stage); manifest.Status = "prepared"; SaveManifest(target, manifest);
                RequireStopped(target); RejectReparse(target);
                if (!Same(PatchCore.Hash(target), currentHash) || !Same(CurrentConfigHash(ConfigPath(target)), config.BeforeHash)) throw new IOException("目标文件或配置在操作期间改变，未提交修复。");
                File.Replace(stage, target, rollbackExe, true); replaced = true;
                rollbackConfig = CommitConfig(target, config);
                PatchCore.VerifyPatched(target);
                if (!Same(PatchCore.Hash(target), manifest.PatchedHash)) throw new IOException("修复后可执行文件校验失败。");
                manifest.Status = "installed"; SaveManifest(target, manifest); completed = true;
                log("原版备份：" + backup);
                return "安装完成。" + (lockUpdates ? "自动更新已锁定（SystemSystemUpdate:3）。" : "未修改自动更新设置。") + "可以点击“启动 PCL”。";
            }
            catch (Exception original)
            {
                List<string> errors = new List<string>();
                try { RollbackConfig(target, config, rollbackConfig); } catch (Exception ex) { errors.Add(ex.Message); }
                if (replaced)
                    try
                    {
                        RequireStopped(target); RejectReparse(target); RejectReparse(rollbackExe);
                        if (!Same(PatchCore.Hash(target), manifest.PatchedHash)) throw new IOException("可执行文件已改变，未覆盖外部修改。");
                        File.Replace(rollbackExe, target, null, true);
                    }
                    catch (Exception ex) { errors.Add(ex.Message); }
                if (errors.Count > 0) throw new IOException(original.Message + "\r\n自动回滚未完全完成，请保留备份与恢复记录：" + String.Join("；", errors.ToArray()), original);
                throw;
            }
            finally { SafeDeleteTemporary(stage); if (completed) { SafeDeleteTemporary(rollbackExe); if (rollbackConfig != "created") SafeDeleteTemporary(rollbackConfig); } }
        }
    }

    public string Restore(string input)
    {
        string target = Normalize(input);
        using (new TargetLock(target))
        {
            RequireStopped(target); RestoreManifest manifest = LoadManifest(target);
            if (manifest == null) throw new IOException("未找到此 PCL 的恢复记录；助手只恢复自己备份和修复的文件。");
            VerifyBackup(target, manifest); string currentHash = PatchCore.Hash(target);
            if (Same(currentHash, manifest.OriginalHash))
            {
                if (manifest.Status == "restored") return "已是原版，无需重复恢复。";
                if (manifest.Status == "restoring" || manifest.Status == "installed")
                {
                    if (manifest.Status == "installed")
                    {
                        ConfigChange reconciled = PlanRestoreConfig(target, manifest);
                        manifest.RestoreConfigBeforeHash = reconciled.BeforeHash; manifest.RestoreConfigAfterHash = reconciled.AfterHash; manifest.RestoreConfigChanged = reconciled.Changed;
                        manifest.Status = "restoring"; SaveManifest(target, manifest);
                    }
                    string currentConfig = CurrentConfigHash(ConfigPath(target));
                    if (manifest.RestoreConfigChanged && !Same(currentConfig, manifest.RestoreConfigAfterHash))
                    {
                        if (!Same(currentConfig, manifest.RestoreConfigBeforeHash)) throw new IOException("上次恢复中断，且配置已发生外部修改，已停止自动覆盖。");
                        ConfigChange pending = PlanRestoreConfig(target, manifest);
                        if (!Same(pending.AfterHash, manifest.RestoreConfigAfterHash)) throw new IOException("恢复事务配置校验失败。");
                        string temporary = CommitConfig(target, pending); if (temporary != "created") SafeDeleteTemporary(temporary);
                    }
                    manifest.Status = "restored"; SaveManifest(target, manifest); return "已完成上次中断的恢复；备份和用户数据均已保留。";
                }
                if (manifest.Status == "prepared" && Same(CurrentConfigHash(ConfigPath(target)), manifest.ConfigBeforeHash))
                { manifest.Status = "restored"; SaveManifest(target, manifest); return "上次安装未提交；原版和设置已确认保留。"; }
                throw new IOException("文件已是原版，但恢复记录不一致；未覆盖设置。");
            }
            if (!Same(currentHash, manifest.PatchedHash)) throw new InvalidDataException("当前文件与本次修复的 SHA-256 不匹配，拒绝恢复以免覆盖更新或其他修改。");
            PatchCore.VerifyPatched(target);
            if (manifest.Status != "installed" && manifest.Status != "prepared" && manifest.Status != "restoring") throw new IOException("恢复记录状态无效。");
            ConfigChange config = PlanRestoreConfig(target, manifest);
            manifest.RestoreConfigBeforeHash = config.BeforeHash; manifest.RestoreConfigAfterHash = config.AfterHash; manifest.RestoreConfigChanged = config.Changed;
            string stage = Temporary(target, "original"), rollbackExe = Temporary(target, "patched-backup"), rollbackConfig = null;
            string priorStatus = manifest.Status; bool replaced = false, completed = false;
            try
            {
                FlushCopy(Path.Combine(BackupPath(target), manifest.BackupFileName), stage);
                if (!Same(PatchCore.Hash(stage), manifest.OriginalHash)) throw new IOException("原版暂存文件校验失败。");
                manifest.Status = "restoring"; SaveManifest(target, manifest);
                RequireStopped(target); RejectReparse(target);
                if (!Same(PatchCore.Hash(target), currentHash) || !Same(CurrentConfigHash(ConfigPath(target)), config.BeforeHash)) throw new IOException("目标文件或设置在恢复期间改变，未提交恢复。");
                File.Replace(stage, target, rollbackExe, true); replaced = true;
                rollbackConfig = CommitConfig(target, config);
                if (!Same(PatchCore.Hash(target), manifest.OriginalHash)) throw new IOException("恢复后的原版文件校验失败。");
                manifest.Status = "restored"; SaveManifest(target, manifest); completed = true;
                if (!String.IsNullOrEmpty(config.Warning)) log(config.Warning);
                return "原版已恢复，安装前的更新设置已恢复（用户随后改动的值除外）。备份和用户数据均已保留。";
            }
            catch (Exception original)
            {
                List<string> errors = new List<string>();
                try { RollbackConfig(target, config, rollbackConfig); } catch (Exception ex) { errors.Add(ex.Message); }
                if (replaced)
                    try
                    {
                        RequireStopped(target); RejectReparse(target); RejectReparse(rollbackExe);
                        if (!Same(PatchCore.Hash(target), manifest.OriginalHash)) throw new IOException("可执行文件已发生外部修改，未覆盖。");
                        File.Replace(rollbackExe, target, null, true);
                    }
                    catch (Exception ex) { errors.Add(ex.Message); }
                if (errors.Count == 0) { manifest.Status = priorStatus; try { SaveManifest(target, manifest); } catch (Exception ex) { errors.Add(ex.Message); } }
                if (errors.Count > 0) throw new IOException(original.Message + "\r\n自动回滚未完全完成，请保留备份：" + String.Join("；", errors.ToArray()), original);
                throw;
            }
            finally { SafeDeleteTemporary(stage); if (completed) { SafeDeleteTemporary(rollbackExe); if (rollbackConfig != "created") SafeDeleteTemporary(rollbackConfig); } }
        }
    }

    public string Inspect(string input)
    {
        string target = Normalize(input);
        using (new TargetLock(target))
        {
            string hash = PatchCore.Hash(target); RestoreManifest manifest = LoadManifest(target);
            if (Same(hash, PatchCore.SupportedHash)) return "支持的原版 PCL 2.99.0.0\r\nSHA-256: " + hash;
            if (manifest != null && Same(hash, manifest.PatchedHash))
            { VerifyBackup(target, manifest); PatchCore.VerifyPatched(target); return "已识别本助手修复的 PCL 2.99.0.0；记录状态：" + manifest.Status + "\r\nSHA-256: " + hash; }
            throw new InvalidDataException("不支持的文件或缺少恢复记录。SHA-256: " + hash);
        }
    }
    public static string AutoDetect()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string parent = Path.GetDirectoryName(basePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        foreach (string directory in new string[] { basePath, parent })
        {
            if (String.IsNullOrEmpty(directory)) continue;
            try
            {
                RejectReparse(directory); int count = 0;
                foreach (string path in Directory.GetFiles(directory, "*.exe", SearchOption.TopDirectoryOnly))
                {
                    if (++count > 40) break;
                    try { new InstallerEngine(null).Inspect(path); return Path.GetFullPath(path); } catch { }
                }
            }
            catch { }
        }
        return null;
    }
}

internal sealed class InstallerForm : Form
{
    private static readonly Color ThemeBlue = Color.FromArgb(0, 102, 204);
    private static readonly Color ThemeBackground = Color.FromArgb(240, 247, 253);
    private TextBox pathBox, logBox;
    private Button browse, install, restore, launch;
    private CheckBox lockUpdates;
    private Label updateChoice;
    private bool busy;
    public InstallerForm()
    {
        Name = "InstallerManager"; Text = "PCL 下载恢复助手 v1a（Kuro_Miaow 第三方修改）";
        StartPosition = FormStartPosition.CenterScreen; AutoScaleMode = AutoScaleMode.Font; AutoScaleDimensions = new SizeF(7F, 17F);
        Font = new Font("Microsoft YaHei UI", 9F); ClientSize = new Size(800, 680); MinimumSize = new Size(720, 640);
        BackColor = ThemeBackground; ForeColor = Color.FromArgb(35, 55, 75);
        TabControl tabs = new TabControl(); tabs.Dock = DockStyle.Fill; tabs.Padding = new Point(18, 8);
        TabPage main = new TabPage("安装与恢复"), about = new TabPage("关于与说明"), help = new TabPage("更新与源码帮助");
        foreach (TabPage page in new TabPage[] { main, about, help }) { page.BackColor = ThemeBackground; page.Padding = new Padding(18); tabs.TabPages.Add(page); }
        Controls.Add(tabs);
        TableLayoutPanel layout = new TableLayoutPanel(); layout.Name = "ManagerLayout"; layout.Dock = DockStyle.Fill; layout.ColumnCount = 1; layout.RowCount = 6;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        foreach (float height in new float[] { 80F, 46F, 184F, 52F, 26F }) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); main.Controls.Add(layout);
        TableLayoutPanel header = new TableLayoutPanel(); header.Dock = DockStyle.Fill; header.BackColor = ThemeBlue; header.Padding = new Padding(14, 6, 14, 6); header.Margin = new Padding(0, 0, 0, 10); header.ColumnCount = 1; header.RowCount = 2;
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); header.RowStyles.Add(new RowStyle(SizeType.Percent, 52F)); header.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
        Label title = TextLabel("PCL 下载恢复助手 v1a · 第一版 · 第三方", Color.White); title.Font = new Font(Font.FontFamily, 15F, FontStyle.Bold); header.Controls.Add(title, 0, 0);
        Label subtitle = TextLabel("适用于已核验的 PCL 2.99.0.0；安装前请退出 PCL，原版会自动备份。", Color.White); header.Controls.Add(subtitle, 0, 1); layout.Controls.Add(header, 0, 0);
        TableLayoutPanel pathRow = new TableLayoutPanel(); pathRow.Dock = DockStyle.Fill; pathRow.Margin = new Padding(0, 0, 0, 10); pathRow.ColumnCount = 2; pathRow.RowCount = 1;
        pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); pathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
        pathBox = new TextBox(); pathBox.Name = "PclPath"; pathBox.Dock = DockStyle.Fill; pathBox.Margin = new Padding(0, 4, 10, 0); pathBox.BackColor = Color.White; pathRow.Controls.Add(pathBox, 0, 0);
        browse = ActionButton("选择 PCL…", false); browse.Dock = DockStyle.Fill; browse.Margin = new Padding(0); browse.Click += Browse; pathRow.Controls.Add(browse, 1, 0); layout.Controls.Add(pathRow, 0, 1);
        TableLayoutPanel options = new TableLayoutPanel(); options.Name = "UpdateOptionsCard"; options.Dock = DockStyle.Fill; options.BackColor = Color.White; options.Padding = new Padding(12, 8, 12, 8); options.Margin = new Padding(0, 0, 0, 10); options.ColumnCount = 1; options.RowCount = 4;
        options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        foreach (float height in new float[] { 28F, 52F, 48F, 28F }) options.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        lockUpdates = new CheckBox(); lockUpdates.Name = "DisablePclUpdates"; lockUpdates.Text = "禁用 PCL 自动更新（勾选为“是”，不推荐）"; lockUpdates.Checked = false; lockUpdates.Dock = DockStyle.Fill; lockUpdates.AutoSize = true; lockUpdates.ForeColor = ThemeBlue; lockUpdates.Font = new Font(Font, FontStyle.Bold); options.Controls.Add(lockUpdates, 0, 0);
        updateChoice = TextLabel("", ForeColor); updateChoice.Name = "UpdateChoiceDescription"; options.Controls.Add(updateChoice, 0, 1);
        Label updateWarning = TextLabel("警告：选“是”会阻止官方更新自动生效，可能错过修复与回滚。建议保持不勾选，让作者后续更新正常覆盖补丁。", Color.FromArgb(192, 0, 0)); updateWarning.Name = "UpdateLockWarning"; options.Controls.Add(updateWarning, 0, 2);
        LinkLabel manual = new LinkLabel(); manual.Name = "UpdateHelpLink"; manual.Text = "如何解除禁用 / 手动修改配置与源码 →"; manual.Dock = DockStyle.Fill; manual.LinkColor = ThemeBlue; manual.ActiveLinkColor = Color.FromArgb(0, 75, 155); manual.TextAlign = ContentAlignment.MiddleLeft;
        manual.LinkClicked += delegate { tabs.SelectedTab = help; }; options.Controls.Add(manual, 0, 3); layout.Controls.Add(options, 0, 2);
        lockUpdates.CheckedChanged += delegate { UpdateChoiceDescription(); }; UpdateChoiceDescription();
        FlowLayoutPanel actions = new FlowLayoutPanel(); actions.Dock = DockStyle.Fill; actions.Margin = new Padding(0); actions.WrapContents = false;
        install = ActionButton("安装并启用", true); install.Size = new Size(150, 40); install.Click += delegate { StartAction(false); }; actions.Controls.Add(install);
        restore = ActionButton("恢复原版", false); restore.Size = new Size(130, 40); restore.Click += delegate { StartAction(true); }; actions.Controls.Add(restore);
        launch = ActionButton("启动 PCL", false); launch.Size = new Size(130, 40); launch.Enabled = false; launch.Click += Launch; actions.Controls.Add(launch); layout.Controls.Add(actions, 0, 3);
        Label logTitle = TextLabel("操作记录", ThemeBlue); logTitle.Font = new Font(Font, FontStyle.Bold); layout.Controls.Add(logTitle, 0, 4);
        logBox = new TextBox(); logBox.Name = "OperationLog"; logBox.Multiline = true; logBox.ReadOnly = true; logBox.ScrollBars = ScrollBars.Vertical; logBox.Dock = DockStyle.Fill; logBox.Margin = new Padding(0); logBox.BackColor = Color.White; logBox.ForeColor = ForeColor; logBox.BorderStyle = BorderStyle.FixedSingle; layout.Controls.Add(logBox, 0, 5);
        TableLayoutPanel aboutLayout = new TableLayoutPanel(); aboutLayout.Dock = DockStyle.Fill; aboutLayout.ColumnCount = 1; aboutLayout.RowCount = 2; aboutLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); aboutLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F)); aboutLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); about.Controls.Add(aboutLayout);
        LinkLabel sponsor = new LinkLabel(); sponsor.Text = "PCL 原作者：龙腾猫跃 · 赞助主页 https://afdian.com/a/LTCat"; sponsor.Dock = DockStyle.Fill; sponsor.LinkColor = ThemeBlue; sponsor.Font = new Font(Font, FontStyle.Bold);
        sponsor.LinkClicked += delegate { try { Process.Start(new ProcessStartInfo("https://afdian.com/a/LTCat") { UseShellExecute = true }); } catch (Exception ex) { MessageBox.Show(this, ex.Message, "无法打开链接", MessageBoxButtons.OK, MessageBoxIcon.Information); } }; aboutLayout.Controls.Add(sponsor, 0, 0);
        TextBox details = ReadOnlyText(); details.Name = "AboutDetails"; details.Text = @"PCL 下载恢复助手 v1a · 第一版 · 独立第三方工具 · 维护：Kuro_Miaow

本工具并非 PCL 官方发布，也不代表原作者。
仅修复已核验版本的下载入口；不触及账号、登录或赞助权限。
原版和恢复记录保存在目标旁的 .pcl-download-restore 文件夹。
请勿删除此文件夹，否则无法安全恢复。

安装完成后由你决定何时启动 PCL。下载仍需网络和上游服务。
游戏及整合包文件不随本工具分发，请遵守对应授权与服务条款。
官方更新或回滚可覆盖补丁；不兼容的新构建会被拒绝修补。

命令行：--install <PCL.exe> [--lock-updates]
               --restore <PCL.exe>    --inspect <PCL.exe>
一键入口：--quick-install [PCL.exe]（不禁用更新）

界面为 PCL 蓝白风格的第三方助手；不会更改 PCL 自身主题。"; aboutLayout.Controls.Add(details, 0, 1);
        TextBox helpText = ReadOnlyText(); helpText.Name = "UpdateManualHelp"; helpText.Text =
            @"更新选项：本次安装是否禁用 PCL 自动更新？

否（默认不勾选）：保留原有更新设置。若此前由本助手加锁，保留备份、在同一路径取消勾选后点击“安装并启用”，会先恢复再安装。你原本就关闭更新的设置不会被强制开启。

是（主动勾选，不推荐）：安装时将目标旁 PCL/Setup.ini 的 SystemSystemUpdate 写为 3，禁用自动更新；可能错过官方修复和回滚。勾选动作本身不改文件，需点击“安装并启用”才生效。此处是本次安装选项，并非已安装状态检测。

推荐解除方法
退出 PCL → 保留 .pcl-download-restore → 在原路径取消勾选 → 安装并启用；也可点击“恢复原版”。如果你后来改过更新键，会保留你的值；请在 PCL 的设置中重新选择所需更新方式。

手动修改配置（不需要改代码）
先退出 PCL，备份目标旁的 PCL/Setup.ini，用文本编辑器定位 SystemSystemUpdate。值 3 表示禁用。解除时优先使用上面的管理操作，或从自己的备份恢复原值 / 原先不存在时删除该行；不要猜测其他数字。保留其他设置与编码，不编辑恢复记录 XML。手动修改后，本助手可能为保护你的选择而保留该值。

手动修改源码（开发者）
一键包进入 source；GitHub 源码根目录直接编辑 src/Installer.cs：
1. InstallerForm：lockUpdates.Checked = false 为界面默认值；UpdateChoiceDescription() 负责是/否说明。
2. InstallerProgram.QuickInstall：Install(target, false) 为一键入口默认值。
3. InstallerProgram.Main：--install 只有收到 --lock-updates 才传入 true。
4. IniDocument.SetLock：SystemSystemUpdate:3 为实际锁定项。保留备份与恢复逻辑。
不建议把任何默认值改为 true；如自行修改，须同步更正文案并重新验证。

构建：在源码根目录运行
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
构建后运行 test.ps1 与 tests/StartupNoticeTests.ps1；重新编译会改变 EXE 哈希，发布前更新 README 和 SHA256SUMS，使用新版本包名。

配色位置：InstallerForm 的 ThemeBlue / ThemeBackground 和 ActionButton()；StartupNotice.cs 使用同系蓝白配色。红色风险提示请保持清晰可见。
官方更新可能覆盖补丁，兼容时可再启用；不兼容时须等待新版助手。"; help.Controls.Add(helpText);
        FormClosing += delegate(object sender, FormClosingEventArgs e) { if (busy) { e.Cancel = true; Append("正在提交或回滚，请等待操作结束后关闭。"); } };
        Shown += delegate
        {
            Append("正在查找同目录或上一级目录内的受支持 PCL……"); SetBusy(true);
            Task.Factory.StartNew(delegate { string found = InstallerEngine.AutoDetect(); BeginInvoke((Action)delegate { SetBusy(false); if (found != null) { pathBox.Text = found; Append("已找到：" + found); } else Append("未自动找到受支持文件，请点击“选择 PCL”。"); }); });
        };
    }
    private void UpdateChoiceDescription()
    {
        updateChoice.Text = lockUpdates.Checked
            ? "是：本次安装将写入 SystemSystemUpdate:3。只在点击“安装并启用”后生效；可能错过官方修复或回滚。"
            : "否（默认）：保留原有更新设置。若已由本助手加锁，取消勾选后再次安装可解除；原先自行关闭的设置会保留。";
    }
    private static Label TextLabel(string text, Color color)
    {
        Label label = new Label(); label.Text = text; label.Dock = DockStyle.Fill; label.ForeColor = color; label.TextAlign = ContentAlignment.MiddleLeft; label.Margin = new Padding(0, 2, 0, 2); return label;
    }
    private static Button ActionButton(string text, bool primary)
    {
        Button button = new Button(); button.Text = text; button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderSize = primary ? 0 : 1; button.FlatAppearance.BorderColor = Color.FromArgb(167, 201, 233); button.BackColor = primary ? ThemeBlue : Color.White; button.ForeColor = primary ? Color.White : ThemeBlue; button.UseVisualStyleBackColor = false; button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(0, 88, 178) : Color.FromArgb(225, 240, 253); button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(0, 72, 148) : Color.FromArgb(208, 230, 250); button.Margin = new Padding(0, 2, 12, 2); return button;
    }
    private TextBox ReadOnlyText()
    {
        TextBox box = new TextBox(); box.Dock = DockStyle.Fill; box.Multiline = true; box.ReadOnly = true; box.ScrollBars = ScrollBars.Vertical; box.BackColor = Color.White; box.ForeColor = ForeColor; box.BorderStyle = BorderStyle.FixedSingle; box.Margin = new Padding(0); box.GotFocus += delegate { box.Select(0, 0); }; return box;
    }
    private void Browse(object sender, EventArgs e)
    {
        using (OpenFileDialog dialog = new OpenFileDialog())
        { dialog.Title = "选择 PCL 2.99.0.0 可执行文件"; dialog.Filter = "PCL 可执行文件 (*.exe)|*.exe"; dialog.CheckFileExists = true; if (dialog.ShowDialog(this) == DialogResult.OK) { pathBox.Text = dialog.FileName; launch.Enabled = false; } }
    }
    private void Append(string message) { logBox.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + message + Environment.NewLine); }
    private void SetBusy(bool value) { busy = value; browse.Enabled = !value; install.Enabled = !value; restore.Enabled = !value; pathBox.Enabled = !value; lockUpdates.Enabled = !value; if (value) launch.Enabled = false; }
    private void StartAction(bool restoring)
    {
        string target = pathBox.Text; bool shouldLock = lockUpdates.Checked; SetBusy(true);
        Task.Factory.StartNew(delegate
        {
            bool success = false;
            try
            {
                InstallerEngine engine = new InstallerEngine(delegate(string text) { BeginInvoke((Action)delegate { Append(text); }); });
                string result = restoring ? engine.Restore(target) : engine.Install(target, shouldLock); success = true;
                BeginInvoke((Action)delegate { Append(result); });
            }
            catch (Exception ex) { string error = ex.Message; BeginInvoke((Action)delegate { Append("失败：" + error); }); }
            finally { bool enabled = success && !restoring; BeginInvoke((Action)delegate { SetBusy(false); launch.Enabled = enabled; }); }
        });
    }
    private void Launch(object sender, EventArgs e)
    {
        try { string target = Path.GetFullPath(pathBox.Text); new InstallerEngine(null).Inspect(target); Process.Start(new ProcessStartInfo(target) { WorkingDirectory = Path.GetDirectoryName(target), UseShellExecute = true }); }
        catch (Exception ex) { Append("无法启动：" + ex.Message); }
    }
}
