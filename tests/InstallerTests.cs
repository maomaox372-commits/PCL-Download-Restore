// Test-only stub. Compile this with Installer.cs, never with the production PatchCore.
using System;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

public static class PatchCore
{
    public static readonly byte[] Original = Encoding.UTF8.GetBytes("pcl-installer-test-original");
    public static readonly byte[] Patched = Encoding.UTF8.GetBytes("pcl-installer-test-patched");
    public static readonly string SupportedHash = InstallerEngine.HashBytes(Original);
    public static readonly string LegacyPatchedHash = InstallerEngine.HashBytes(Encoding.UTF8.GetBytes("legacy-stub-not-current"));
    public static bool FailPatch;
    public static bool FailAfterCommit;
    public static void Patch(string input, string output)
    {
        if (FailPatch) throw new IOException("Injected patch failure");
        if (Hash(input) != SupportedHash) throw new IOException("bad input");
        File.WriteAllBytes(output, Patched);
    }
    public static void VerifyPatched(string path)
    {
        if (Hash(path) != InstallerEngine.HashBytes(Patched)) throw new IOException("bad patch");
        if (FailAfterCommit && Path.GetFileName(path) == "PCL-test.exe") throw new IOException("Injected after-commit failure");
    }
    public static string Hash(string path) { using (SHA256 sha = SHA256.Create()) using (FileStream f = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(f)).Replace("-", ""); }
}

internal static class InstallerTests
{
    private static int count;
    private static string root;
    private static InstallerEngine engine = new InstallerEngine(null);
    private static void Assert(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); count++; Console.WriteLine("PASS: " + name); }
    private static string Setup(string name, byte[] ini)
    {
        string folder = Path.Combine(root, name); Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "PCL-test.exe"); File.WriteAllBytes(path, PatchCore.Original);
        if (ini != null) { Directory.CreateDirectory(Path.Combine(folder, "PCL")); File.WriteAllBytes(Path.Combine(folder, "PCL", "Setup.ini"), ini); }
        return path;
    }
    private static string Ini(string target) { return Path.Combine(Path.GetDirectoryName(target), "PCL", "Setup.ini"); }
    private static bool Equal(byte[] a, byte[] b) { return InstallerEngine.HashBytes(a) == InstallerEngine.HashBytes(b); }
    private static void Fails(Action action, string name) { try { action(); } catch { Assert(true, name); return; } throw new Exception("Expected rejection: " + name); }
    private static byte[] Bytes(string s) { return new UTF8Encoding(false).GetBytes(s); }
    private static void TestEncoding(string name, Encoding encoding)
    {
        byte[] text = encoding.GetBytes("名字:克洛\r\nSystemSystemUpdate:1\r\nOther:留住\r\n");
        byte[] preamble = encoding.GetPreamble(), original = new byte[preamble.Length + text.Length];
        Buffer.BlockCopy(preamble, 0, original, 0, preamble.Length); Buffer.BlockCopy(text, 0, original, preamble.Length, text.Length);
        string target = Setup(name, original); engine.Install(target, true); engine.Restore(target);
        Assert(Equal(original, File.ReadAllBytes(Ini(target))), name + " exact roundtrip");
    }
    private static int Main(string[] args)
    {
        if (args.Length == 3 && args[0] == "--hold")
        {
            using (EventWaitHandle exit = EventWaitHandle.OpenExisting(args[1])) { File.WriteAllText(args[2], "ready"); exit.WaitOne(15000); }
            return 0;
        }
        root = Path.Combine(Path.GetDirectoryName(typeof(InstallerTests).Assembly.Location), "installer-fixtures-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try
        {
            string target = Setup("normal", Bytes("Alpha:one\r\nSystemSystemUpdate:1\r\nBeta:two\r\n"));
            engine.Install(target, true); Assert(PatchCore.Hash(target) == InstallerEngine.HashBytes(PatchCore.Patched), "install patches target");
            Assert(File.ReadAllText(Ini(target)).Contains("SystemSystemUpdate:3\r\n"), "install locks update key");
            byte[] installedIni = File.ReadAllBytes(Ini(target)); engine.Install(target, true);
            Assert(Equal(installedIni, File.ReadAllBytes(Ini(target))), "repeat install preserves settings");
            File.AppendAllText(Ini(target), "Later:user-edit\r\n", new UTF8Encoding(false)); engine.Restore(target);
            Assert(PatchCore.Hash(target) == PatchCore.SupportedHash, "restore original bytes");
            Assert(File.ReadAllText(Ini(target)) == "Alpha:one\r\nSystemSystemUpdate:1\r\nBeta:two\r\nLater:user-edit\r\n", "restore preserves subsequent unrelated settings");
            engine.Restore(target); Assert(true, "repeat restore is idempotent");
            Assert(File.Exists(Path.Combine(Path.GetDirectoryName(target), ".pcl-download-restore", PatchCore.SupportedHash + ".original.exe")), "backup retained");
            target = Setup("missing-key", Bytes("Alpha:one")); engine.Install(target, true); engine.Restore(target);
            Assert(File.ReadAllText(Ini(target)) == "Alpha:one", "missing key and no trailing newline roundtrip");
            target = Setup("missing-file", null); engine.Install(target, true); engine.Restore(target);
            Assert(File.ReadAllBytes(Ini(target)).Length == 0, "created config restores absent key without deleting user file");
            target = Setup("no-lock", Bytes("Other:x\nSystemSystemUpdate:2\n")); byte[] before = File.ReadAllBytes(Ini(target)); engine.Install(target, false); engine.Restore(target);
            Assert(Equal(before, File.ReadAllBytes(Ini(target))), "no-update-lock preserves bytes");
            target = Setup("unlock-on-reinstall", Bytes("SystemSystemUpdate:1\nOther:keep\n")); engine.Install(target, true); engine.Install(target, false);
            Assert(File.ReadAllText(Ini(target)) == "SystemSystemUpdate:1\nOther:keep\n", "reinstall without lock removes tool-created update lock");
            engine.Restore(target); Assert(PatchCore.Hash(target) == PatchCore.SupportedHash, "unlock migration remains restorable");
            target = Setup("unlock-user-edit", Bytes("SystemSystemUpdate:1\n")); engine.Install(target, true); File.WriteAllText(Ini(target), "SystemSystemUpdate:2\nLater:keep\n", new UTF8Encoding(false)); engine.Install(target, false);
            Assert(File.ReadAllText(Ini(target)) == "SystemSystemUpdate:2\nLater:keep\n", "unlock migration preserves later user selection");
            target = Setup("unlock-failure", Bytes("SystemSystemUpdate:1\n")); engine.Install(target, true); PatchCore.FailPatch = true; string migrationTarget = target;
            Fails(delegate { engine.Install(migrationTarget, false); }, "migration surfaces new patch failure"); PatchCore.FailPatch = false;
            Assert(PatchCore.Hash(target) == PatchCore.SupportedHash && File.ReadAllText(Ini(target)) == "SystemSystemUpdate:1\n", "failed migration leaves working unlocked original");
            target = Setup("default-missing-config", null); engine.Install(target, false); engine.Restore(target);
            Assert(!File.Exists(Ini(target)), "unlocked install and restore do not create config");
            target = Setup("upstream-update", Bytes("SystemSystemUpdate:1\nOther:keep\n")); engine.Install(target, false); File.WriteAllText(target, "official-future-update"); string futureTarget = target;
            Fails(delegate { engine.Restore(futureTarget); }, "restore refuses to overwrite official update");
            Fails(delegate { engine.Install(futureTarget, false); }, "install refuses to reapply over unknown official update");
            Assert(File.ReadAllText(target) == "official-future-update" && File.ReadAllText(Ini(target)) == "SystemSystemUpdate:1\nOther:keep\n", "official update bytes and settings remain untouched");
            target = Setup("upstream-rollback", Bytes("SystemSystemUpdate:1\n")); engine.Install(target, true); File.WriteAllBytes(target, PatchCore.Original); engine.Restore(target);
            Assert(PatchCore.Hash(target) == PatchCore.SupportedHash && File.ReadAllText(Ini(target)) == "SystemSystemUpdate:1\n", "upstream rollback to original reconciles tracked lock without replacing EXE");
            target = Setup("reenable-supported-rollback", Bytes("SystemSystemUpdate:1\n")); engine.Install(target, false); File.WriteAllBytes(target, PatchCore.Original); engine.Install(target, false);
            Assert(PatchCore.Hash(target) == InstallerEngine.HashBytes(PatchCore.Patched) && File.ReadAllText(Ini(target)) == "SystemSystemUpdate:1\n", "manual re-enable after supported official rollback leaves updates enabled");
            target = Setup("user-key-change", Bytes("SystemSystemUpdate:1\n")); engine.Install(target, true); File.WriteAllText(Ini(target), "SystemSystemUpdate:2\nOther:user\n", new UTF8Encoding(false)); engine.Restore(target);
            Assert(File.ReadAllText(Ini(target)) == "SystemSystemUpdate:2\nOther:user\n", "restore preserves user-changed update key");
            TestEncoding("utf8-bom", new UTF8Encoding(true)); TestEncoding("utf16le", new UnicodeEncoding(false, true)); TestEncoding("utf16be", new UnicodeEncoding(true, true)); TestEncoding("utf32le", new UTF32Encoding(false, true)); TestEncoding("utf32be", new UTF32Encoding(true, true)); TestEncoding("gbk", Encoding.GetEncoding(936));
            target = Setup("tampered-target", null); engine.Install(target, false); File.AppendAllText(target, "tamper"); string captured = target; Fails(delegate { engine.Restore(captured); }, "tampered target refused");
            target = Setup("tampered-backup", null); engine.Install(target, false); File.AppendAllText(Path.Combine(Path.GetDirectoryName(target), ".pcl-download-restore", PatchCore.SupportedHash + ".original.exe"), "tamper"); captured = target; Fails(delegate { engine.Restore(captured); }, "tampered backup refused");
            target = Setup("unknown", null); File.WriteAllText(target, "unknown"); captured = target; Fails(delegate { engine.Install(captured, true); }, "unknown version refused");
            target = Setup("duplicate-key", Bytes("SystemSystemUpdate:1\r\nSystemSystemUpdate:2\r\n")); captured = target; Fails(delegate { engine.Install(captured, true); }, "duplicate key refused"); Assert(PatchCore.Hash(target) == PatchCore.SupportedHash, "duplicate rejection leaves original");
            target = Setup("patch-failure", Bytes("SystemSystemUpdate:1\r\n")); captured = target; PatchCore.FailPatch = true; Fails(delegate { engine.Install(captured, true); }, "patch failure surfaces"); PatchCore.FailPatch = false; Assert(PatchCore.Hash(target) == PatchCore.SupportedHash && File.ReadAllText(Ini(target)) == "SystemSystemUpdate:1\r\n", "precommit failure leaves files");
            target = Setup("commit-failure", Bytes("SystemSystemUpdate:1\r\n")); captured = target; PatchCore.FailAfterCommit = true; Fails(delegate { engine.Install(captured, true); }, "postcommit failure surfaces"); PatchCore.FailAfterCommit = false; Assert(PatchCore.Hash(target) == PatchCore.SupportedHash && File.ReadAllText(Ini(target)) == "SystemSystemUpdate:1\r\n", "postcommit rollback restores exe and setting"); engine.Install(target, true); Assert(PatchCore.Hash(target) == InstallerEngine.HashBytes(PatchCore.Patched), "retry after rollback succeeds");
            target = Setup("prepared-resume", Bytes("SystemSystemUpdate:1\r\n")); engine.Install(target, true); string manifest = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(target), ".pcl-download-restore"), "manifest-*.xml")[0]; File.WriteAllText(manifest, File.ReadAllText(manifest).Replace("<Status>installed</Status>", "<Status>prepared</Status>"), new UTF8Encoding(false)); File.WriteAllText(Ini(target), "SystemSystemUpdate:1\r\n", new UTF8Encoding(false)); engine.Install(target, true); Assert(File.ReadAllText(Ini(target)) == "SystemSystemUpdate:3\r\n", "interrupted install resumes configuration");
            target = Setup("restoring-resume", Bytes("SystemSystemUpdate:1\r\n")); engine.Install(target, true); engine.Restore(target); manifest = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(target), ".pcl-download-restore"), "manifest-*.xml")[0]; File.WriteAllText(manifest, File.ReadAllText(manifest).Replace("<Status>restored</Status>", "<Status>restoring</Status>"), new UTF8Encoding(false)); File.WriteAllText(Ini(target), "SystemSystemUpdate:3\r\n", new UTF8Encoding(false)); engine.Restore(target); Assert(File.ReadAllText(Ini(target)) == "SystemSystemUpdate:1\r\n", "interrupted restore resumes configuration");
            target = Setup("parallel-lock", null); captured = target;
            using (ManualResetEvent entered = new ManualResetEvent(false)) using (ManualResetEvent release = new ManualResetEvent(false))
            {
                Thread holder = new Thread(delegate() { using (new TargetLock(captured)) { entered.Set(); release.WaitOne(); } }); holder.Start(); entered.WaitOne();
                try { Fails(delegate { engine.Install(captured, false); }, "concurrent target mutex refused"); } finally { release.Set(); holder.Join(); }
            }
            string liveDir = Path.Combine(root, "running-target"); Directory.CreateDirectory(liveDir);
            string liveExe = Path.Combine(liveDir, "PCL-test.exe"), ready = Path.Combine(liveDir, "ready.txt"), eventName = "Local\\PCLInstallerTest." + Guid.NewGuid().ToString("N");
            File.Copy(typeof(InstallerTests).Assembly.Location, liveExe);
            using (EventWaitHandle leave = new EventWaitHandle(false, EventResetMode.ManualReset, eventName))
            using (Process child = Process.Start(new ProcessStartInfo(liveExe, "--hold " + eventName + " \"" + ready + "\"") { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden }))
            {
                try
                {
                    DateTime deadline = DateTime.UtcNow.AddSeconds(5); while (!File.Exists(ready) && DateTime.UtcNow < deadline) Thread.Sleep(25);
                    Assert(File.Exists(ready), "running target fixture ready");
                    try { engine.Install(liveExe, false); throw new Exception("Running process not rejected"); }
                    catch (IOException ex) { Assert(ex.Message.Contains("正在运行"), "running target rejected before mutation"); }
                }
                finally { leave.Set(); child.WaitForExit(5000); }
            }
            Console.WriteLine("ALL PASSED: " + count + " checks. Fixtures: " + root); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); Console.Error.WriteLine("Fixtures retained: " + root); return 1; }
    }
}
