# RuntimeProbe 隔离验证复现

[English](runtime-reproduce.md) | **简体中文**

环境：Windows x64 + .NET Framework 4.8；使用系统 csc.exe 以 C# 5 编译。不得把 RuntimeProbe 放进用户的真实游戏/启动器目录。

前置文件（分享包不包含 PCL 或游戏二进制）：
1. 用户自行提供受支持的官方 PCL 2.99.0.0，原始 SHA256 为 6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386。
2. 复制到独立测试目录，对副本用本工具生成补丁结果；1.0.2 结果 SHA256 为 B3061C3E8D95520655045B535817FC524D319C1763C13036F2F903EC65998D05；旧 1.0.0 / 1.0.1 结果为 2FF3A197EDA8F7E9DF707A0CE910A4338D21E898359E09846938984E877A2A90。
3. 同目录需要 RuntimeProbe.cs、runtime-reproduce.ps1、runtime-curseforge-manifest.json、runtime-modrinth-index.json；需提供 Mono.Cecil 0.11.6 的 net40/Mono.Cecil.dll。

PowerShell 执行示例（调整为自己的绝对路径）：

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\runtime-reproduce.ps1 -PatchedExe D:\IsolatedTest\PCL.exe -CecilDll D:\Cecil\net40\Mono.Cecil.dll

仅 JIT 和线上按钮分支验证，不下载游戏：加 -SkipDownloads。
完整测试会下载 Minecraft 1.20.1 资源与库，约 750 MB，之后重复测试会重用已下载的文件。脚本只生成 tests/runtime-work、runtime-lib 和两个带明确测试标记的本地整合包，不修改真实 PCL 安装。

宿主载入原 PCL 内嵌依赖，创建 PCL.Application 并加载 WPF 资源。为隔离桌面启动流程，移除宿主对象上的 Startup/异常事件并清空其 StartupUri；不调用 PCL Application_Startup 或 FormMain 构造、不读取账号和密钥、不更改生产程序集。整合包入口所需 FrmMain 只使用未初始化的字段容器，加真实 MyExtraButton 提供空 ShowRefresh 路径，绝不显示窗口。下载、校验、整合包解析与安装方法仍是补丁 PCL 原有实现。

测试包是验证下载和安装的数据，不能作为可玩整合包发布：
- CurseForge：MC 1.20.1、Fabric 0.16.14，Wick 项目 1728334 / 文件 9092522。项目与文件来自 PCL 原 DlModRequest 对 CurseForge 官方 API 的公开元数据查询，API 密钥未读出或导出。
- Modrinth：MC 1.20.1、Fabric 0.16.14，Sodium 0.5.13。下载地址/大小/SHA1来自 Modrinth 官方版本 OihdIimA 元数据。
- 测试未启动 Minecraft，不证明上述测试模组依赖齐全或游戏内兼容。

2026-10-20 历史通过项目：五个补丁方法 JIT；Mojang 官方版本列表；原版 jar+json+64 项适用于 Windows 的库+3575 个资源对象及资源索引；CF 无模组及真实模组安装；Modrinth真实Sodium+Fabric安装；线上ModPack与普通Mod分支委托；Settings.Get<int>(SystemSystemUpdate,null)真实读取3。

本次验收详情：runtime-summary.json。原版客户端、资源索引、3575 个资源对象的 SHA1均匹配官方元数据；64项Windows库SHA1均匹配，其余24项Linux/macOS专用库按平台规则跳过。线上按钮分支通过实际调用 InitializeStackResourceVersions 验证，未实际点击或登录启动游戏。