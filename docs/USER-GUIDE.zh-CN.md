# PCL 下载恢复助手（独立第三方）

[English](USER-GUIDE.md) | **简体中文**

PCL 原作者：**龙腾猫跃**。支持原作者：https://afdian.com/a/LTCat 。

这是针对 **PCL 正式版 2.99.0.0** 的独立第三方本地修补工具，不是 PCL 官方插件。它恢复已有下载入口，并补回发布时被编译器移除的整合包分发逻辑。修复后继续使用 PCL 原来的界面、下载器、版本隔离、模组加载器安装及账号体系。

正式发布版本 **v1a（第一版）** · 第三方维护：**Kuro_Miaow**。

## 支持正版 Minecraft

建议通过 https://www.minecraft.net/ 购买正版 Minecraft，并支持 PCL 原作者。工具不提供游戏授权。每次启动本助手（含命令行）先显示正版与赞助提示，只有“确定(Y)”按钮；点击、Y、Alt+Y 或 Enter 确认后继续，没有不再提示选项。确认仅表示已阅读。

**官方更新或回滚可直接覆盖本补丁，助手默认不锁更新。未来 PCL2 更新后，可能需要安装新版脚本，或在兼容时再次运行当前脚本启用一次。当前脚本仅支持已核验构建；新版本不兼容时会拒绝修补，不会强行覆盖。助手不会自动阻止更新，也不会自动重打补丁。**

## 文件完整性与发布来源

本工具发布仓库：https://github.com/maomaox372-commits/PCL-Download-Restore 。发布值应从该仓库的同版本 Releases 核对，不能只相信转发者提供的截图或哈希。

**v1a 本次分发的 PCL.DownloadRestore.exe SHA-256：**

    FFCAA53E99BE9DD56E74A8FB67ACCE66C79E14E3EBFE7DD03F710BEE72BF9AF3

**若本地文件的 SHA-256 与仓库同版本、同文件的发布值不一致，请立刻停止使用并删除该文件，再从仓库重新获取。不要继续点“确定”或运行。**

请先在 PowerShell 中核验，再启动：

    Get-FileHash -LiteralPath './PCL.DownloadRestore.exe' -Algorithm SHA256

启动窗口会计算并显示当前助手 EXE 的 SHA-256，并显示仓库地址及不一致警告。**这个自报值不等于已通过可信校验，不能证明程序未被替换；须与独立取得的发布值核对。** EXE 与 ZIP 是不同文件，哈希不能互相比较。源码自行重编译的 EXE 可能有不同哈希，不应冒充本次分发二进制。

包内 SHA256SUMS.txt 列出脚本、依赖和源码等各文件的哈希；ZIP 最终哈希见外置 README 与发布记录。本包未数字签名，哈希核验也不等于数字签名。

## 一键使用

1. **退出 PCL，把整个压缩包解压。** 不要在压缩软件的预览窗口中运行，也不要只取走一个 EXE。
2. 双击 **一键安装启用.cmd**，先确认正版提示。能在同目录或上一级找到受支持 PCL 时会直接安装；否则只需在弹窗中选择 `Plain Craft Launcher 2.exe`。也可把 PCL 的 EXE 拖到这个 CMD 上。
3. 看到“安装完成”后，照常打开原来的 PCL，使用“下载 → 自动安装”“下载 → 整合包”或导入本地包。修复器不会自动启动或结束 PCL。

想选择是否锁定更新，或还原：双击 **管理与恢复.cmd**，选择同一个 PCL，点击“安装并启用”或“恢复原版”。一键入口、默认 CLI 和管理窗口均默认不锁更新，不写 SystemSystemUpdate:3；管理窗口有红色风险提示、实时是/否说明和“更新与源码帮助”页；点击“安装并启用”后才应用选项。详见 [更新选项与手动修改](UPDATE-OPTIONS.zh-CN.md)。只有主动勾选或 --lock-updates 才会加锁，不建议使用。

已有内部测试版安装请保留恢复记录并在原路径运行新版默认安装；会先恢复旧版及本工具施加的锁，再安装新版。用户后续选择保留；迁移安装失败时留下原版。用户原本关闭更新的设置不会被强制开启。修补器不会改写官方更新日志；历史更新弹窗仍可能显示“删除下载功能”，实际状态以下载页为准。

无需安装 Python、Node.js 或开发工具。运行环境是 Windows + .NET Framework 4.8，与 PCL 2.99.0 使用的运行时一致。不申请管理员权限；目标目录需要当前账户有写入权限。

## 恢复范围

- 原版版本列表和安装流程，包括原有 Forge / NeoForge / Fabric 等组件选项。
- 在线整合包版本项恢复“安装”，同时保留“另存为”。普通 Mod、资源包的点击行为不变。
- 本地整合包导入与拖入安装：CurseForge、Modrinth `.mrpack`、HMCL、MMC、MCBBS、含整合包的启动器压缩包，以及 PCL 原来可识别的普通压缩包。
- 已安装实例的 Minecraft 版本选择卡片。

实际完成的联网测试与未覆盖项见 [TEST-REPORT.md](../TEST-REPORT.zh-CN.md)。不承诺任意第三方整合包都能下载：已删除文件、上游网络故障、平台限制自动下载、损坏清单或 PCL 原先不兼容的组件仍可能失败。这些情况不是只解除本地入口即可解决的。

## 精确兼容性

仅接受原始文件 SHA-256：

`6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386`

必须是与此指纹完全一致的 **2.99.0.0 正式版**，不是所有标注 2.99.0 的构建都通用。旧版、预览版、魔改版和未来版本将被拒绝，避免误修。工具不自动下载或替换成另一个启动器。

## 备份与还原

安装时在目标 EXE 旁创建 `.pcl-download-restore`，保留原版 EXE、哈希与恢复记录；不删除 `.minecraft`、版本、存档、账号或其他用户文件。请保留这个备份目录。

安装和还原均核验文件，检测正在运行的 PCL，并使用暂存文件、原子替换及回滚。恢复仅替换本工具修改过的文件；如果后来手动替换、再次更新或篡改，会停止而不是覆盖。还原只处理本工具改过的自动更新键，保留后来对其他设置的修改；若该更新键后来被你改成其他值，也保留你的值。

恢复记录绑定目标的完整路径。如需移动或重命名 PCL，请先恢复原版，移动后重新安装；仅复制已修复 EXE 而不带恢复记录，无法正常管理或还原。

修补后的 PCL 主窗口加载会弹出可见的“Kuro_Miaow 第三方修改”提示；关闭提醒后继续正常加载。

工具没有修改账号校验、正版劝导、赞助功能、网络证书或安全软件设置。压缩包不包含 Minecraft 游戏文件、整合包、任何用户账号或官方 PCL 可执行文件。

## 开发与命令行

一键包内源码位于 `source`；GitHub 源码目录本身即构建根目录。在该目录运行 `powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1` 可用 Windows 自带的 .NET Framework C# 编译器重新编译；此命令仅对本次构建进程设置执行策略。无需 NuGet 在线恢复，Mono.Cecil 0.11.6 随包附带。

```text
PCL.DownloadRestore.exe --quick-install [PCL.exe 的路径]
PCL.DownloadRestore.exe --install "D:\游戏\PCL.exe"
PCL.DownloadRestore.exe --install "D:\游戏\PCL.exe" --lock-updates
PCL.DownloadRestore.exe --restore "D:\游戏\PCL.exe"
PCL.DownloadRestore.exe --inspect "D:\游戏\PCL.exe"
```

所有 CLI 入口（含 --help）都需人工确认正版提示，不能用于无人值守批处理。CLI `--install` / `--restore` / `--inspect` 成功返回 0，失败返回非 0；`--quick-install` 是带文件选择及结果弹窗的一键入口。

## 来源与署名

PCL 官方源码：https://github.com/Meloong-Git/PCL 。限制变更固定提交：`e4b0489a22cfd4bc57a18fc1e2ac66d5c18e7fc1`。

第三方独立二次创作，补丁源码随包公开。PCL 的《分发有限许可》及《存储库合理使用指南》保留在 `licenses/PCL-LICENCE.txt`；Mono.Cecil 的 MIT 许可证保留在 `licenses/Mono.Cecil-LICENSE.txt`。仅分享修补工具，不捆绑分发官方程序或游戏数据，不暗示官方支持。

## 安全提示与许可状态补充

本版本未数字签名，检测为 NotSigned；Kuro_Miaow 是维护者署名，不是 Windows 已验证发布者。系统仍可能显示安全提示，详见 [安全说明](SECURITY-WARNINGS.zh-CN.md)。上游指南和本项目落实记录见 [LICENSE-STATUS.md](../LICENSE-STATUS.zh-CN.md)。普通用户不要运行 source/tests 中的开发测试。
