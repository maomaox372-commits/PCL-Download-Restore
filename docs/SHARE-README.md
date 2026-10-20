# PCL 下载恢复助手 v1a（第一版 · 独立第三方）

[English](SHARE-README.en.md) | **简体中文**

**PCL 原作者：龙腾猫跃 · 支持原作者：https://afdian.com/a/LTCat**

第三方维护：**Kuro_Miaow**。本文件与 **PCL-Download-Restore-v1a.zip** 放在同一网盘目录。工具是第三方基于 PCL 独立进行二次创作的产物，不是 PCL 官方插件。

## 支持正版

建议通过 https://www.minecraft.net/ 购买并支持正版 Minecraft，同时欢迎支持 PCL 原作者。本工具不提供游戏授权。每次启动助手都会先显示正版与赞助提醒，仅有“确定(Y)”按钮；点击或按 Y / Alt+Y / Enter 后继续。确认仅表示已阅读。

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

## 使用步骤

1. 退出 PCL，完整解压 ZIP；不要在压缩软件内运行，不要只拿走 EXE。
2. 双击“一键安装启用.cmd”，确认正版提示；未自动找到兼容 PCL 时选择自己的 PCL EXE。也可以把 EXE 拖到 CMD 上。
3. 安装完成后手动打开 PCL，使用原版自动安装、在线整合包或本地整合包导入。

需要 Windows + .NET Framework 4.8、目标目录写权限，无需开发环境或管理员权限。普通用户不要运行 source/tests。

## 精确兼容性

仅支持 PCL 2.99.0.0 的指定原始构建，SHA-256：

    6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386

同版本号但指纹不同也不能使用。恢复原版、在线/本地整合包分发及原有加载器选项；已删除文件、平台限制、网络故障和损坏清单仍可能失败。

## 未签名与安全提示

**本版本未数字签名，状态 NotSigned。** Kuro_Miaow 是维护者署名，不是 Windows 已验证发布者。Windows 仍可能提示未知发布者或拦截；新版已修正开发测试反复启动助手的方式，不保证消除系统提示。助手内的“正版支持提示”与系统警告不同。

先核对来源和下方哈希。不要关闭安全软件或安装所谓信任证书；反复出现系统提醒时先取消并核对文件路径。完整安全说明在 ZIP 内 source/docs/SECURITY-WARNINGS.zh-CN.md。

## 恢复与更新

双击“管理与恢复.cmd”，确认提示后选择同一个 PCL 并点击“恢复原版”。保留目标旁的 .pcl-download-restore 备份目录。

所有入口默认不锁更新，管理窗口采用蓝白配色，提供红色警告、是/否实时说明；主动勾选锁更新会阻止官方自动更新，不建议使用。旧版用户保留原路径及备份，运行新版默认安装会解除本工具曾施加的锁并升级补丁，保留用户后续设置。用户原先关闭更新的设置不会被强制开启。默认无配置时不新建文件；显式加锁创建的配置恢复后留空文件。移动或重命名前先恢复。

要自行选择禁用更新：运行“管理与恢复.cmd”，勾选“禁用 PCL 自动更新”（默认不勾选），再点击“安装并启用”。取消勾选后在原路径再次安装可解除本助手施加的锁，用户原本自行关闭的设置保留。帮助链接与“更新与源码帮助”页包含配置/源码修改方法；完整文档位于 ZIP 内 source/docs/UPDATE-OPTIONS.zh-CN.md。该选项仅代表本次安装选择，不检测当前状态。

修补后的 PCL 自身启动会显示“Kuro_Miaow 第三方修改”文字，确认后继续加载。

## 验证与边界

v1a 的 43 项安装事务检查、60 项界面/启动提示检查、真实生产引擎加锁/解除/恢复回归和六方法差异审计通过。界面默认值和补丁核心保持不变；旧版迁移与 JIT 结果见内部调试历史报告。此前 2026-10-20 已在隔离环境完成 Minecraft 1.20.1、Modrinth 和 CurseForge 非空测试包下载/安装。本轮未重复联网下载，未登录或进入游戏，未覆盖所有整合包。详见 ZIP 内 TEST-REPORT.zh-CN.md。

## 核对 ZIP

- 文件名：PCL-Download-Restore-v1a.zip
- 大小：{{ZIP_SIZE}} 字节
- SHA-256：{{ZIP_SHA256}}

PowerShell：

    Get-FileHash -LiteralPath './PCL-Download-Restore-v1a.zip' -Algorithm SHA256

ZIP 内还有逐文件 SHA256SUMS.txt。源码目录中的本文件是外置说明模板，打包后填入最终大小与哈希，避免 ZIP 把自己的哈希包含在自身内部。

## 源码、来源与转发

ZIP 的 source 含完整工具源码，可审阅并构建；另有独立 GitHub 源码发布目录。项目仓库：https://github.com/maomaox372-commits/PCL-Download-Restore 。本轮仅准备本地 v1a 材料，是否已上传此版本以仓库为准。GitHub 的 Source code ZIP 是源码，普通用户应下载 Releases 中的一键包。

PCL 官方源码：https://github.com/Meloong-Git/PCL 。包内 source/LICENCE 与 licenses/PCL-LICENCE.txt 原样保留上游固定指南；source/LICENSE-STATUS.md 记录具体落实情况。继续遵循上游文件，不声称获得额外官方授权或背书。Mono.Cecil 的 MIT 仅适用于该依赖。

转发请同时保留本 README、包内源码、许可与署名，不加入 PCL 本体、游戏、账号或证书；不要宣传为官方版本、绝对安全或全版本通用。反馈时提供脱敏错误和文件哈希，不上传账号资料。
