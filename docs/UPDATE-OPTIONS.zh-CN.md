# 更新选项、手动修改与蓝白配色

[English](UPDATE-OPTIONS.md) | **简体中文**

PCL 原作者：龙腾猫跃 · 支持原作者：https://afdian.com/a/LTCat 。维护者：Kuro_Miaow，独立第三方工具 v1a。

## 在界面中选择

完整解压一键包后运行 **管理与恢复.cmd**，选择自己的受支持 PCL。安装页提供唯一的“禁用 PCL 自动更新”复选框，默认不勾选；旁边的说明随勾选实时变化。

- **否（默认）**：本次安装不施加更新锁，保留原有更新设置。用户原先已经关闭更新的选择不会被强制开启。
- **是（主动勾选，不推荐）**：安装时把目标旁 `PCL/Setup.ini` 的 `SystemSystemUpdate` 写为 `3`。**这会禁用官方自动更新，可能错过作者的修复和回滚。**
- 勾选动作本身不写文件，必须点击 **安装并启用** 才应用。本复选框代表本次安装选项，不是当前安装状态检测；重新打开助手时仍默认不勾选。

**建议保持默认，让官方更新或回滚正常覆盖补丁。未来 PCL2 更新后，可能需要安装新版脚本，或在兼容时再次运行当前脚本启用；未知构建会被拒绝修补。** 一键入口和默认 CLI 均不禁用更新。

## 如何解除本助手的锁

1. 退出 PCL，保留原路径及 `.pcl-download-restore` 备份目录。
2. 打开管理界面，选择同一个 PCL，取消勾选，再点击 **安装并启用**。已有本工具锁记录时会先恢复再安装；迁移第二步失败时保留原版。
3. 也可使用 **恢复原版**，同时恢复本工具曾修改的更新设置。用户后续更改过该键时保留用户值；若仍关闭更新，请在 PCL 设置中自行重新选择。

不要删除备份或编辑恢复记录 XML。安装器不提供后台守护，不拦截上游更新，不自动重打补丁。

## 手动改配置

此操作不需要改代码。优先采用管理界面，避免破坏配置和恢复记录的一致性。

1. 完全退出 PCL，备份目标 EXE 旁的 `PCL/Setup.ini`。
2. 用文本编辑器定位 `SystemSystemUpdate`，值 `3` 表示禁用。
3. 解除时从**自己的原备份**恢复原值；原先不存在该键时可删除该行。**不要猜测其他数字的意义。** 也可在 PCL 自身设置中重新选择更新方式。
4. 保留其他键、文件编码和换行；不要编辑 `.pcl-download-restore` 中的恢复记录。手动修改后，本助手可能为了保留用户选择而不再还原该键。

## 手动改源码

一键包进入 `source`；GitHub 源码根目录直接操作。编辑 `src/Installer.cs`，按方法名定位：

- `InstallerForm`：`lockUpdates.Checked = false` 是 GUI 默认值；`UpdateChoiceDescription()` 更新是/否文案。
- `InstallerProgram.QuickInstall`：`Install(target, false)` 是一键入口默认值。
- `InstallerProgram.Main`：CLI 只有显式收到 `--lock-updates` 才传入 `true`，保留旧 `--no-update-lock` 兼容。
- `IniDocument.SetLock`：写入 `SystemSystemUpdate:3`；备份与恢复逻辑仍由安装引擎管理。

不建议把任何默认值改为 `true`。如果自行调整，务必同步 GUI/帮助/README 文案、测试预期和版本号，不能只改显示文字。不要修改更新地址、上游更新判断、版本号检测或哈希白名单来绕过兼容性保护。

在源码根目录重新构建和测试：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./build.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./tests/StartupNoticeTests.ps1
```

重新编译会改变 EXE 哈希；同步所有当前版本 README/Release 哈希，再用新版本号打包。窗口显示自身哈希不能证明来源，用户需要运行前核对仓库独立发布值。自编译文件不能沿用发布二进制的哈希。

## 蓝白配色的位置

助手仍为 WinForms，保留系统标题栏。安装页采用 PCL 风格蓝色标题与主按钮、浅蓝背景、白色内容卡片、蓝色次按钮边框和帮助链接。不会更改 PCL 本身的主题，不代表官方界面。

- `InstallerForm.ThemeBlue`：主色 `#0066CC`。
- `InstallerForm.ThemeBackground`：背景 `#F0F7FD`。
- `ActionButton()`：按钮边框、悬停与按下配色。
- `src/StartupNotice.cs`：同色系启动提示；仍只有“确定(Y)”一个按钮。
- 风险文字保持红色 `#C00000`，不要为统一配色而降低警告可见性。

布局采用 `TableLayoutPanel` 和停靠，说明可换行，帮助可滚动。默认与最小窗口经过离屏检查；不同显示缩放、系统主题和字体仍可能改变实际渲染。