# 实现与审计范围

[English](TECHNICAL.md) | **简体中文**

PCL 原作者：龙腾猫跃，https://afdian.com/a/LTCat 。本项目为独立第三方二次创作。

## 版本与来源

- 目标：PCL 2.99.0.0，原始 SHA-256 6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386。
- 限制提交：https://github.com/Meloong-Git/PCL/commit/e4b0489a22cfd4bc57a18fc1e2ac66d5c18e7fc1
- 参考父提交：5ef81e490ae46b1100d61e33d0183094c7a80ca8。
- 依赖：Mono.Cecil 0.11.6 net40。

RestrictMcDownload 是编译期常量；优化后部分正常分支已经消失，所以不能只改配置或跳过一条异常指令。PatchCore.cs 在严格 SHA-256 白名单与结构核验后，重建受影响的 IL。

## 恰好六个修改方法（1.0.2）

1. PageDownloadInstall.LoaderInit()：接回原版列表加载器。
2. ModModpack.ModpackInstall()：恢复文件选择和已有后台导入委托。
3. ModModpack.ModpackInstall(string,string,string)：恢复七种整合包分发及清理/返回逻辑。
4. PageDownloadResourceDetail.InitializeStackResourceVersions(StackPanel)：恢复整合包安装主操作及保存次操作，普通 Mod 仍保存。
5. PageDownloadModify.LoadInstanceFinished(...)：恢复 Minecraft 组件卡片。
6. FormMain.FormMain_Loaded(...)：在原有主窗口加载逻辑前增加可见的第三方修改提醒，保留后续原有指令。

1.0.2 二进制审计：6,339 个方法中仅这 6 个改变，其他 6,333 个方法指令语义一致，原资源和引用保留，仅另加第三方标记。不能将这个结果泛化到其他 PCL 构建。

## 安装事务

Installer.cs 负责路径/进程校验、同目标互斥、原始文件和恢复记录、暂存/刷盘/原子替换、失败回滚及中断续接。默认不改配置；用户显式加锁时仅改 SystemSystemUpdate，保留原编码、换行及其他设置。旧版迁移经同目标互斥，先运行原版恢复事务，再安装新版；第二阶段失败时保留原版。未知更新文件拒绝覆盖。

## 验证层次

- test.ps1 默认编译 Installer.cs + StartupNotice.cs + InstallerTests.cs，后者含 PatchCore 测试桩，验证安装事务而非真实补丁。
- InstallerIntegration.ps1 使用独立反射宿主调用真实生产安装引擎，操作原版文件的隔离副本，验证安装和恢复。
- Audit.ps1 对比真实原版和修补后文件的六方法、资源及引用。
- RuntimeProbe.cs 是隔离 .NET Framework / WPF 反射宿主，调用原 PCL 下载和整合包安装方法。未登录或进入游戏。

新增目标须重新完成上述层次，不复用旧结论。原始 PCL 和游戏资源由测试者自行取得，不在仓库或 CI 中分发。

## 1.0.1 交互入口与测试分层

InstallerProgram.Main 初始化 WinForms 后创建唯一 StartupNotice；ShowDialog 返回 OK 才开始解析命令或进入管理界面。所有参数（包括 --help）都经过提示；没有跳过参数或持久化“不再提示”设置。原 PCL 的游戏启动劝导保持不变。

StartupNotice.cs 实现正版购买、PCL 赞助与第三方署名；AssemblyInfo.cs 标记 Kuro_Miaow 及 1.0.1.0。这些元数据不是 Authenticode 数字签名。

新版 InstallerIntegration.ps1 通过 ProductionEngineProbe 反射调用真实生产程序集的 InstallerEngine，独立验证安装事务；StartupNoticeTests.ps1 检查真实 Main 的门控与弹窗交互。测试入口未编入生产包 EXE，不用于绕过生产提示。TestHost.config 仅对测试宿主允许加载待测程序集，不改变生产或系统配置。

## 1.0.2 更新、可见标识与哈希

默认 GUI、一键入口与 CLI 均 lockUpdates=false；--no-update-lock 兼容，--lock-updates 显式加锁。旧精确哈希补丁在同目标互斥下先 Restore 再 Install；并非跨两步的原子事务，第二步失败时留下原版。未知官方更新拒绝覆盖，没有后台守护或自动重打补丁。

FormMain_Loaded 增加六条前置 IL，调用已有 PresentationFramework 的 WPF MessageBox，确认后继续全部原指令；不增加引用，不改更新、账号或赞助判断。原五个下载修复方法与 1.0.1 语义相同。

StartupNotice 计算其自身程序集文件的 SHA-256，不嵌入最终自身哈希，避免自引用。固定分发 EXE 哈希写在 README/Release。自报哈希不能认证文件，必须与独立发布记录核对；没有自动信任或自动删除操作。

## 1.0.3 界面与帮助

安装引擎和 PatchCore 未变。InstallerForm 改用 TableLayoutPanel、停靠与最小窗口限制，ThemeBlue=#0066CC、ThemeBackground=#F0F7FD，白卡片和红色警告。唯一复选框默认为 false，CheckedChanged 仅更新说明；StartAction 才读取选择并调用既有 Install。复选框不是状态探测器。

更新与源码帮助页提供配置备份、解除锁、四处源码定位、构建和哈希更新说明；关于页保留原作者首位链接。StartupNotice 仅统一背景和按钮悬停配色，单确认与哈希算法不变。详情见 [更新选项](UPDATE-OPTIONS.md)。