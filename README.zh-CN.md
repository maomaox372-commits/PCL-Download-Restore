# PCL 下载恢复助手 v1a（独立第三方）

**PCL 原作者：龙腾猫跃 · [支持原作者](https://afdian.com/a/LTCat) · [官方仓库](https://github.com/Meloong-Git/PCL)**

[English](README.md) | **简体中文** | [繁體中文（香港）](README.zh-HK.md) | [繁體中文（台灣）](README.zh-TW.md)

**正式发布版本：v1a（第一版）**。维护者：**Kuro_Miaow**。正式版本号以实际正式发布为准，此前数字版本均为内部调试迭代。

本项目是第三方基于 PCL 独立进行二次创作的产物，为 **PCL 2.99.0.0 的一个精确构建**恢复原版 Minecraft 与整合包下载入口。不是 PCL 官方插件，不代表原作者。继续使用 PCL 原有下载器、组件安装、版本隔离及账号体系；不分发官方或修改后的 PCL EXE、游戏、模组或账号数据。桌面界面仍为简体中文，多语言版本仅针对文档。

## 支持正版与原作者

**建议通过 [Minecraft 官方渠道](https://www.minecraft.net/) 购买正版游戏，并欢迎[支持 PCL 原作者](https://afdian.com/a/LTCat)。** 工具不提供游戏授权。每次启动助手（含 CLI 和 --help）先显示单个 **确定(Y)** 按钮提示，可点击或按 Y、Alt+Y、Enter。没有跳过或不再提示选项，确认仅表示已阅读。

**官方更新或回滚可直接覆盖本补丁，助手默认不禁用更新。未来 PCL2 更新后，可能需要安装新版脚本，或在兼容时再次运行当前脚本启用一次。当前仅支持已核验构建，不兼容时拒绝修补；不会阻止官方覆盖，也不会自动重打补丁。**

修补后的 PCL 主窗口加载时也会显示“Kuro_Miaow 第三方修改”，明确非官方身份。

## 下载与使用

在仓库 **Releases → Assets** 下载 **PCL-Download-Restore-v1a.zip**。GitHub 自动生成的 Source code ZIP 与 Code → Download ZIP 是源码，需自行编译。网盘分享为同一份一键 ZIP 加独立简中 README。

1. 退出 PCL，完整解压，不要在压缩软件预览中运行。
2. 双击 **一键安装启用.cmd**，阅读提示并确认。未找到兼容文件时选择自己的 PCL EXE，也可把 EXE 拖到 CMD 上。
3. 完成后手动启动 PCL，使用原版自动安装、在线整合包或本地导入入口。

**管理与恢复.cmd** 提供选项与还原。“禁用 PCL 自动更新”默认不勾选；勾选为“是”，永久红色警告说明可能错过官方修复和回滚。仅点击 **安装并启用** 后生效，复选框不检测当前安装状态。只有主动勾选或 --lock-updates 才写入 SystemSystemUpdate:3。

解除本工具施加的锁：退出 PCL、保留原路径和 .pcl-download-restore，取消勾选并再次安装。已有恢复/安装流程会移除记录中的锁，保留用户后续选择。用户原本自行关闭的更新不会被强制开启；未知官方更新文件不会被覆盖。详见[更新与手动修改](docs/UPDATE-OPTIONS.zh-CN.md)。

管理窗口使用蓝色标题与按钮、浅蓝底、白卡片及红色警告。需要 Windows、.NET Framework 4.8 和目标目录写权限；使用时不需要 Python、Node.js 或管理员权限。详见[用户指南](docs/USER-GUIDE.zh-CN.md)。

## 运行前核对完整性

发布仓库：https://github.com/maomaox372-commits/PCL-Download-Restore 。请从独立取得的发布记录核对**同版本、同文件**的值。

**v1a PCL.DownloadRestore.exe SHA-256：**

    FFCAA53E99BE9DD56E74A8FB67ACCE66C79E14E3EBFE7DD03F710BEE72BF9AF3

**哈希不一致时立刻停止使用并删除文件，再从仓库重新获取，不要继续运行。**

    Get-FileHash -LiteralPath './PCL.DownloadRestore.exe' -Algorithm SHA256
    Get-FileHash -LiteralPath './PCL-Download-Restore-v1a.zip' -Algorithm SHA256

启动窗口计算当前 EXE 的哈希。**自报哈希不能证明文件可信**，须在运行前与独立发布值核对；EXE 与 ZIP 的哈希不同，自行编译也可能不同。包内 SHA256SUMS.txt 提供逐文件值，ZIP 最终值见外置 README 或发布清单。

**本版本未数字签名，状态 NotSigned。** Kuro_Miaow 是署名，不是 Windows 已验证发布者。系统仍可能显示安全警告或 SmartScreen；不安装信任证书，不应关闭安全软件或修改全局信任策略。详见[安全提示](docs/SECURITY-WARNINGS.zh-CN.md)。

## 精确兼容性与范围

仅接受原始 PCL EXE SHA-256：

    6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386

版本号相同但指纹不同也不支持。恢复原版列表和已有 Forge/NeoForge/Fabric 选项、在线整合包安装/另存为、本地 CurseForge/Modrinth/HMCL/MMC/MCBBS 等 PCL 原可识别的包导入，以及已安装实例的 Minecraft 卡片。普通模组资源保持原保存行为。

资源下架、平台限制、网络、损坏清单或不兼容组件仍可导致失败，不能保证任意整合包成功。不修改账号、购买提醒、赞助权限、PCL 主题、证书或安全软件。

## 备份与恢复

保留目标旁 .pcl-download-restore。安装/恢复核验哈希与路径，拒绝运行中的目标，使用同目标互斥、暂存、原子替换与失败回滚。保留后来修改的其他设置和更新值，未知或篡改文件拒绝覆盖。

恢复记录绑定原完整路径：**移动或重命名前先恢复，移动后再安装。** 默认安装不创建原本不存在的 Setup.ini；仅因加锁创建的配置恢复后留空文件，不删除用户路径。

## 验证与开发

最终检查与边界见[测试报告](TEST-REPORT.zh-CN.md)。历史隔离测试完成 Minecraft 1.20.1 及非空 Modrinth/CurseForge 包下载；本轮文档与改名不重复联网下载、登录或进入游戏。

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./build.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./test.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./tests/StartupNoticeTests.ps1

使用系统 .NET Framework C# 编译器，随附固定 Mono.Cecil 0.11.6 net40 DLL，无需在线 NuGet。程序集/文件版本 **1.0.3.0** 仅用于内部调试，对外 ProductVersion 与标签为 **v1a**。重新编译后须同步文档哈希；打包器会拒绝旧 EXE 哈希和已存在的同名包。

CLI：--quick-install [PCL.exe]、--install <PCL.exe> [--lock-updates]、--restore <PCL.exe>、--inspect <PCL.exe>。兼容旧 --no-update-lock。所有模式须确认提示，不支持无人值守；成功返回 0，失败非 0，一键取消选择为 2。

## 来源、许可与反馈

原样保留上游 [LICENCE](LICENCE) 并继续遵循，不是项目整体 MIT/GPL 授权；见[许可记录](LICENSE-STATUS.zh-CN.md)与[署名](NOTICE.zh-CN.md)。Mono.Cecil 单独适用 [MIT](licenses/Mono.Cecil-LICENSE.txt)，不声称额外官方授权或背书。

第三方工具问题请在本仓库附脱敏步骤、版本、哈希；不要上传账号、令牌、存档或整套 PCL。见[贡献](CONTRIBUTING.zh-CN.md)、[安全](SECURITY.zh-CN.md)、[发布](docs/PUBLISHING.zh-CN.md)、[v1a 文案](docs/releases/v1a.zh-CN.md)、[技术](docs/TECHNICAL.zh-CN.md)与[运行时复现](tests/runtime-reproduce.zh-CN.md)。配套文档提供英文与简中。