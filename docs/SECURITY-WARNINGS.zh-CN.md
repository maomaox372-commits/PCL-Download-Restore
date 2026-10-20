# Windows 安全提示与启动提醒

[English](SECURITY-WARNINGS.md) | **简体中文**

## v1a 未签名发布

PCL.DownloadRestore.exe 经 Get-AuthenticodeSignature 检查为 **NotSigned**。文件属性和界面中的 **Kuro_Miaow** 是维护者署名，不是 Windows 验证的发布者。维护者决定不进行代码签名，本包没有自签名证书、信任证书安装或 Azure 配置。

Windows 可能因来源和策略出现“打开文件 — 安全警告”“未知发布者”或 SmartScreen 提示。这些不等于检测到病毒，也不能用没有警告来证明安全。此版本不承诺消除系统安全提醒。

## 正版提示与系统提示的区别

每次启动本助手，会出现标题含“正版支持提示”的蓝白窗口，建议购买正版 Minecraft、支持 PCL 原作者，仅有“确定(Y)”按钮。这是助手自身的阅读提醒，点击、Y、Alt+Y 或 Enter 确认后进入原流程。

该提示不会要求安装证书、提升权限或改变系统安全设置。每次独立启动显示一次；关闭后重新运行仍会显示。所有命令行入口也遵循此规则，没有不再提示选项。

## 之前连续弹窗的来源与修正

此前截图路径含 tests/zip-smoke/PCL-Download-Restore-1.0.0，属于开发回归目录。旧测试每轮经 Shell 启动生产助手 7 次，多轮测试可能触发多串安全提示；助手本身没有自动重启循环。

1.0.1 将自动测试拆分为：独立宿主调用真实生产 InstallerEngine，以及离屏检查真实 StartupNotice 和生产入口。测试宿主通过 UseShellExecute=false 启动，捕获输出并限制等待时间，不反复启动交互式生产 Main。

测试宿主的 TestHost.config 仅用于 .NET 反射加载本地测试程序集；生产 App.config、系统策略和来源标记未变。原截图对应文件当时未发现 Zone.Identifier，不能断言所有历史提醒均由这一标记导致。

旧 1.0.0 ZIP 保留原样；新 1.0.1 包携带上述新版测试。普通用户不要运行 source/tests。

## 文件完整性与发布来源

本工具发布仓库：https://github.com/maomaox372-commits/PCL-Download-Restore 。发布值应从该仓库的同版本 Releases 核对，不能只相信转发者提供的截图或哈希。

**v1a 本次分发的 PCL.DownloadRestore.exe SHA-256：**

    FFCAA53E99BE9DD56E74A8FB67ACCE66C79E14E3EBFE7DD03F710BEE72BF9AF3

**若本地文件的 SHA-256 与仓库同版本、同文件的发布值不一致，请立刻停止使用并删除该文件，再从仓库重新获取。不要继续点“确定”或运行。**

请先在 PowerShell 中核验，再启动：

    Get-FileHash -LiteralPath './PCL.DownloadRestore.exe' -Algorithm SHA256

启动窗口会计算并显示当前助手 EXE 的 SHA-256，并显示仓库地址及不一致警告。**这个自报值不等于已通过可信校验，不能证明程序未被替换；须与独立取得的发布值核对。** EXE 与 ZIP 是不同文件，哈希不能互相比较。源码自行重编译的 EXE 可能有不同哈希，不应冒充本次分发二进制。

包内 SHA256SUMS.txt 列出脚本、依赖和源码等各文件的哈希；ZIP 最终哈希见外置 README 与发布记录。本包未数字签名，哈希核验也不等于数字签名。

## 用户检查

1. 从可信发布记录取得 ZIP，核对同目录外置 README 或 Release 中的 SHA-256。
2. 完整解压再运行。如果已确认信任来源，可查看 ZIP 的“右键 → 属性”；存在“解除锁定”选项时自行决定是否使用，再解压到新目录。无此选项时跳过。
3. 系统仍连续提示时先取消，记录提示中的路径，确认没有重复点击或运行测试脚本。
4. 不关闭 Defender、SmartScreen、UAC，不修改全局注册表/策略，不对整盘解除阻止。

    Get-FileHash -LiteralPath './PCL-Download-Restore-v1a.zip' -Algorithm SHA256

ZIP 内 SHA256SUMS.txt 可用于逐文件一致性检查；哈希只能核对内容，不能代替发布者身份及安全判断。

## 官方参考

- https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/unblock-file
- https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.useshellexecute?view=netframework-4.8

## v1a 提示边界

窗口自报哈希不能证明文件未被替换，用户需在运行前独立比对仓库同版本同文件值。不依赖可被同时替换的本地清单自动信任，也不自动删除程序。自行重编译的哈希可能不同，不能套用本次分发值。
