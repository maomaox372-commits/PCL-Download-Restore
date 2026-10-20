using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class StartupNoticeTests
{
    private static int checks;
    private static void Assert(bool value, string label)
    { if (!value) throw new Exception("FAIL: " + label); checks++; Console.WriteLine("PASS: " + label); }
    private static IEnumerable<Control> Descendants(Control root)
    { foreach (Control c in root.Controls) { yield return c; foreach (Control nested in Descendants(c)) yield return nested; } }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2) throw new ArgumentException("Usage: <installer.exe> <preview.png>");
            string exe = Path.GetFullPath(args[0]);
            using (AssemblyDefinition model = AssemblyDefinition.ReadAssembly(exe))
            {
                var main = model.EntryPoint;
                var instructions = main.Body.Instructions;
                var constructors = instructions.Where(x => x.OpCode == OpCodes.Newobj && x.Operand is MethodReference && ((MethodReference)x.Operand).DeclaringType.Name == "StartupNotice").ToList();
                var shown = instructions.Where(x => x.Operand is MethodReference && ((MethodReference)x.Operand).Name == "ShowDialog").ToList();
                Assert(constructors.Count == 1 && shown.Count == 1, "production Main creates one notice and calls ShowDialog once");
                int gate = instructions.IndexOf(shown[0]);
                Assert(!instructions.Take(gate).Any(x => x.OpCode.FlowControl == FlowControl.Cond_Branch), "all arguments encounter the gate before command branching");
                var ops = instructions.Where(x => x.Operand is MethodReference && new[] { "QuickInstall", "Install", "Restore", "Inspect", "Run" }.Contains(((MethodReference)x.Operand).Name)).ToList();
                Assert(ops.Count >= 5 && ops.All(x => instructions.IndexOf(x) > gate), "management, quick install and CLI operations follow the gate");
                Assert(instructions.Skip(gate + 1).Take(6).Any(x => x.OpCode.FlowControl == FlowControl.Cond_Branch), "dialog result is checked before continuing");
            }
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            Assembly loaded = Assembly.LoadFrom(exe);
            var metadata = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe);
            Assert(metadata.ProductVersion == "v1a" && metadata.FileVersion == "1.0.3.0" && loaded.GetName().Version.ToString() == "1.0.3.0", "public v1a and internal 1.0.3 versions are distinct");
            Type noticeType = loaded.GetType("StartupNotice", true);
            using (Form manager = (Form)Activator.CreateInstance(loaded.GetType("InstallerForm", true), true))
            {
                var controls = Descendants(manager).ToList();
                Assert(controls.OfType<CheckBox>().Count() == 1 && !controls.OfType<CheckBox>().Single().Checked, "management update lock defaults off");
                var warning = controls.OfType<Label>().Single(x => x.Name == "UpdateLockWarning");
                Assert(warning.ForeColor.R == 192 && warning.ForeColor.G == 0 && warning.Text.Contains("官方更新"), "management shows permanent red update warning");
                manager.StartPosition = FormStartPosition.Manual; manager.Location = new Point(-20000, -20000); manager.ShowInTaskbar = false; manager.Show();
                DateTime deadline = DateTime.UtcNow.AddSeconds(10);
                while (controls.OfType<CheckBox>().Single().Enabled == false && DateTime.UtcNow < deadline) { Application.DoEvents(); System.Threading.Thread.Sleep(20); }
                Application.DoEvents();
                Assert(manager.BackColor == Color.FromArgb(240, 247, 253), "management uses light blue theme");
                var option = controls.OfType<CheckBox>().Single();
                var choice = controls.OfType<Label>().Single(x => x.Name == "UpdateChoiceDescription");
                Assert(choice.Text.StartsWith("否（默认）") && choice.Text.Contains("原先自行关闭"), "off description preserves existing preferences");
                option.Checked = true; Application.DoEvents();
                Assert(choice.Text.StartsWith("是：") && choice.Text.Contains("SystemSystemUpdate:3") && choice.Text.Contains("点击"), "checked description explains opt-in and apply timing");
                Assert(warning.Visible && warning.Text.Contains("回滚"), "checked choice retains visible red rollback warning");
                using (Bitmap preview = new Bitmap(manager.Width, manager.Height))
                { manager.DrawToBitmap(preview, new Rectangle(Point.Empty, preview.Size)); preview.Save(Path.Combine(Path.GetDirectoryName(args[1]), "installer-updates-disabled.png")); }
                option.Checked = false; Application.DoEvents();
                Assert(choice.Text.StartsWith("否（默认）"), "unchecking immediately restores off description");
                var help = controls.OfType<TextBox>().Single(x => x.Name == "UpdateManualHelp");
                Assert(help.ReadOnly && help.ScrollBars == ScrollBars.Vertical && help.Text.Contains("备份") && help.Text.Contains("不要猜测其他数字") && help.Text.Contains("不编辑恢复记录 XML"), "manual config help is scrollable and preserves settings");
                Assert(help.Text.Contains("lockUpdates.Checked = false") && help.Text.Contains("Install(target, false)") && help.Text.Contains("--lock-updates") && help.Text.Contains("IniDocument.SetLock") && help.Text.Contains("build.ps1"), "source help covers all defaults, key and rebuild");
                var tabs = controls.OfType<TabControl>().Single();
                var link = controls.OfType<LinkLabel>().Single(x => x.Name == "UpdateHelpLink");
                typeof(LinkLabel).GetMethod("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(link, new object[] { new LinkLabelLinkClickedEventArgs(link.Links[0]) });
                Application.DoEvents();
                Assert(tabs.SelectedTab.Text == "更新与源码帮助" && help.Visible, "help link opens the source and config guide");
                using (Bitmap preview = new Bitmap(manager.Width, manager.Height))
                { manager.DrawToBitmap(preview, new Rectangle(Point.Empty, preview.Size)); preview.Save(Path.Combine(Path.GetDirectoryName(args[1]), "installer-update-help.png")); }
                tabs.SelectedIndex = 1; Application.DoEvents();
                var sponsor = Descendants(tabs.SelectedTab).OfType<LinkLabel>().Single();
                var details = controls.OfType<TextBox>().Single(x => x.Name == "AboutDetails");
                Assert(manager.Text.Contains("v1a") && details.Text.Contains("v1a · 第一版") && !details.Text.Contains("1.0.3"), "management presents only the public release version");
                Assert(sponsor.Text.Contains("龙腾猫跃") && sponsor.Text.Contains("https://afdian.com/a/LTCat") && sponsor.Top < details.Top, "about keeps original author sponsorship first");
                tabs.SelectedIndex = 0;
                Size original = manager.Size; manager.Size = manager.MinimumSize; Application.DoEvents();
                var log = controls.OfType<TextBox>().Single(x => x.Name == "OperationLog");
                Assert(log.Height >= 100 && log.Bottom <= log.Parent.ClientSize.Height, "minimum window retains usable log without overlap");
                foreach (Label label in new[] { warning, choice })
                {
                    Size needed = TextRenderer.MeasureText(label.Text, label.Font, new Size(label.Width, Int32.MaxValue), TextFormatFlags.WordBreak);
                    Assert(label.Height >= needed.Height && label.Parent.ClientRectangle.Contains(label.Bounds), "minimum window fits " + label.Name);
                }
                using (Bitmap preview = new Bitmap(manager.Width, manager.Height))
                { manager.DrawToBitmap(preview, new Rectangle(Point.Empty, preview.Size)); preview.Save(Path.Combine(Path.GetDirectoryName(args[1]), "installer-minimum-size.png")); }
                manager.Size = original; Application.DoEvents();
                using (Bitmap preview = new Bitmap(manager.Width, manager.Height))
                { manager.DrawToBitmap(preview, new Rectangle(Point.Empty, preview.Size)); preview.Save(Path.Combine(Path.GetDirectoryName(args[1]), "installer-update-warning.png")); }
            }
            string[] modes = { "button", "Y", "AltY", "Enter" };
            for (int i = 0; i < modes.Length; i++)
            {
                using (Form notice = (Form)Activator.CreateInstance(noticeType, true))
                {
                    var controls = Descendants(notice).ToList();
                    var buttons = controls.OfType<Button>().ToList();
                    Assert(notice.Text.Contains("v1a") && !notice.Text.Contains("1.0.3"), modes[i] + ": startup title uses public v1a");
                    Assert(buttons.Count == 1 && buttons[0].Text == "确定(&Y)", modes[i] + ": exactly one confirm button");
                    Assert(notice.AcceptButton == buttons[0] && notice.CancelButton == null && !notice.ControlBox, modes[i] + ": no cancel or title-bar close action");
                    Assert(!controls.Any(x => x is CheckBox || x is LinkLabel || x is TextBox), modes[i] + ": no opt-out or second interactive action");
                    string labels = String.Join(" ", controls.OfType<Label>().Select(x => x.Text));
                    Assert(labels.Contains("建议支持正版 Minecraft") && labels.Contains("https://www.minecraft.net/") && labels.Contains("https://afdian.com/a/LTCat") && labels.Contains("Kuro_Miaow"), modes[i] + ": expected purchase, sponsorship and attribution text");
                    Assert(labels.Contains("官方更新或回滚可直接覆盖") && labels.Contains("新版脚本") && labels.Contains("不兼容时会拒绝修补"), modes[i] + ": update replacement and compatible re-enable notice");
                    string expectedHash;
                    using (var stream = File.OpenRead(exe))
                    using (var sha = System.Security.Cryptography.SHA256.Create()) expectedHash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
                    Assert(labels.Contains(expectedHash) && labels.Contains("https://github.com/maomaox372-commits/PCL-Download-Restore") && labels.Contains("立刻停止使用并删除") && buttons[0].Enabled, modes[i] + ": exact executable SHA-256, repository and mismatch warning");
                    notice.StartPosition = FormStartPosition.Manual; notice.Location = new Point(-20000, -20000); notice.ShowInTaskbar = false;
                    Exception failure = null; int phase = 0;
                    using (Timer timer = new Timer())
                    {
                        int current = i; timer.Interval = 70;
                        timer.Tick += delegate
                        {
                            try
                            {
                                if (phase++ == 0)
                                {
                                    notice.Close();
                                    Assert(!notice.IsDisposed && notice.Visible, modes[current] + ": closing cannot bypass acknowledgment");
                                    if (current == 0)
                                    {
                                        using (Bitmap image = new Bitmap(notice.Width, notice.Height)) { notice.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size)); image.Save(args[1]); }
                                    }
                                    return;
                                }
                                timer.Stop();
                                if (modes[current] == "button") buttons[0].PerformClick();
                                else if (modes[current] == "Enter")
                                {
                                    MethodInfo dialogKey = typeof(Form).GetMethod("ProcessDialogKey", BindingFlags.NonPublic | BindingFlags.Instance);
                                    dialogKey.Invoke(notice, new object[] { Keys.Enter });
                                }
                                else
                                {
                                    Message message = Message.Create(IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
                                    Keys key = modes[current] == "Y" ? Keys.Y : Keys.Alt | Keys.Y;
                                    noticeType.GetMethod("ProcessCmdKey", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(notice, new object[] { message, key });
                                }
                                Assert(notice.DialogResult == DialogResult.OK, modes[current] + ": explicit confirmation accepted");
                            }
                            catch (Exception ex) { failure = ex; timer.Stop(); buttons[0].PerformClick(); }
                        };
                        notice.Shown += delegate { timer.Start(); };
                        DialogResult result = notice.ShowDialog();
                        if (failure != null) throw failure;
                        Assert(result == DialogResult.OK, modes[i] + ": modal returns OK");
                    }
                }
            }
            Console.WriteLine("ALL PASSED: " + checks + " startup notice checks. Production entry point was inspected, not automated.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
