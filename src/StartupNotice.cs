using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

// Shown on every production entry point; no persisted opt-out or skip switch.
internal sealed class StartupNotice : Form
{
    private bool accepted;
    private readonly Button confirm;
    public StartupNotice()
    {
        Name = "GenuineMinecraftNotice";
        Text = "PCL 下载恢复助手 v1a（Kuro_Miaow 第三方修改） · 正版支持提示";
        Font = new Font("Microsoft YaHei UI", 9F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScaleDimensions = new SizeF(7F, 17F);
        ClientSize = new Size(700, 640);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        ControlBox = false; MinimizeBox = false; MaximizeBox = false;
        ShowInTaskbar = true;
        BackColor = Color.FromArgb(240, 247, 253);
        Padding = new Padding(24);
        TableLayoutPanel layout = new TableLayoutPanel();
        layout.Dock = DockStyle.Fill; layout.ColumnCount = 1; layout.RowCount = 7;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        Controls.Add(layout);
        AddText(layout, 0, "PCL 原作者：龙腾猫跃\r\n支持原作者：https://afdian.com/a/LTCat", 9F, FontStyle.Bold, Color.FromArgb(35, 67, 110));
        AddText(layout, 1, "建议支持正版 Minecraft", 15F, FontStyle.Bold, Color.FromArgb(0, 102, 204));
        AddText(layout, 2, "建议通过 Minecraft 官方渠道购买并支持正版游戏：\r\nhttps://www.minecraft.net/\r\n\r\n官方更新或回滚可直接覆盖本补丁，助手默认不锁更新。\r\n未来 PCL2 更新后，可能需安装新版脚本，或在兼容时再次运行\r\n当前脚本启用。当前仅支持已核验构建，不兼容时会拒绝修补。\r\n助手不会阻止官方覆盖，也不会自动重打补丁。\r\n\r\n本工具不提供游戏授权，也不替代正版购买。\r\n同时欢迎支持 PCL 原作者的持续开发。\r\n点击确定仅表示已阅读此提示，不表示你已购买游戏。", 10F, FontStyle.Regular, Color.FromArgb(40, 40, 40));
        string fileHash = null;
        try
        {
            using (FileStream file = File.OpenRead(typeof(StartupNotice).Assembly.Location))
            using (SHA256 sha = SHA256.Create()) fileHash = BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
        }
        catch (Exception) { }
        AddText(layout, 3, "当前助手 EXE 的 SHA-256（请与仓库同版本发布值核对）：\r\n" + (fileHash ?? "读取失败，请停止使用并重新获取。"), 9F, FontStyle.Regular, Color.FromArgb(40, 40, 40));
        AddText(layout, 4, "发布仓库：https://github.com/maomaox372-commits/PCL-Download-Restore\r\n哈希不一致，请立刻停止使用并删除该文件，再从仓库重新获取。\r\n本窗口显示的是当前文件计算值，不能自行证明文件可信。\r\n请在运行前用 PowerShell 核对仓库 Releases 的同版本 EXE / ZIP 哈希。", 9F, FontStyle.Bold, Color.FromArgb(192, 0, 0));
        AddText(layout, 5, "第三方基于 PCL 独立进行二次创作 · 维护：Kuro_Miaow", 9F, FontStyle.Regular, Color.FromArgb(85, 85, 85));
        confirm = new Button(); confirm.Name = "ConfirmGenuineNotice";
        confirm.Text = "确定(&Y)"; confirm.UseMnemonic = true;
        confirm.Size = new Size(136, 38); confirm.Anchor = AnchorStyles.Right;
        confirm.BackColor = Color.FromArgb(0, 102, 204); confirm.ForeColor = Color.White;
        confirm.FlatStyle = FlatStyle.Flat; confirm.FlatAppearance.BorderSize = 0; confirm.UseVisualStyleBackColor = false;
        confirm.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 88, 178);
        confirm.Click += delegate { accepted = true; DialogResult = DialogResult.OK; Close(); };
        confirm.Enabled = fileHash != null;
        layout.Controls.Add(confirm, 0, 6); AcceptButton = confirm;
        Shown += delegate { confirm.Select(); };
    }
    private static void AddText(TableLayoutPanel layout, int row, string text, float size, FontStyle style, Color color)
    {
        Label label = new Label(); label.Text = text; label.Dock = DockStyle.Fill;
        label.Font = new Font("Microsoft YaHei UI", size, style); label.ForeColor = color;
        label.TextAlign = ContentAlignment.MiddleLeft; label.TabStop = false;
        label.Margin = new Padding(0, 2, 0, 2); layout.Controls.Add(label, 0, row);
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Y || keyData == (Keys.Alt | Keys.Y)) { confirm.PerformClick(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!accepted && e.CloseReason == CloseReason.UserClosing) e.Cancel = true;
        base.OnFormClosing(e);
    }
}
