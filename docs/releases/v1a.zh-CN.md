# PCL 下载恢复助手 v1a（第一版 · Kuro_Miaow 独立第三方）

**PCL 原作者：龙腾猫跃 · 支持原作者：https://afdian.com/a/LTCat**

[English](v1a.md) | **简体中文**

维护者：Kuro_Miaow。仓库：https://github.com/maomaox372-commits/PCL-Download-Restore 。v1a 是首个正式版，数字版本仅为内部调试迭代，正式状态以实际发布为准。

- 在指定 PCL 2.99.0.0 构建恢复原版与在线/本地整合包入口。
- 蓝白管理界面提供禁用更新选项，默认关闭，附红色风险提示与配置/源码帮助。
- 备份恢复、官方更新保护及修补后 PCL 可见第三方标识。
- 每次启动助手先显示正版支持、更新覆盖/重新启用、EXE 哈希和仓库警告。
- README 默认英文，另有简中、香港繁中、台湾繁中；配套文档为英文与简中。

**官方更新或回滚可直接覆盖本补丁。未来 PCL2 更新后，可能需新版脚本，或在兼容时再次运行当前脚本启用。未知构建拒绝修补，默认不禁用更新，不自动重打补丁；保留用户原有更新选择。**

**v1a PCL.DownloadRestore.exe SHA-256：**

    FFCAA53E99BE9DD56E74A8FB67ACCE66C79E14E3EBFE7DD03F710BEE72BF9AF3

**哈希不一致请立刻停用并删除，再从仓库获取。** 自报哈希不能证明可信；运行前独立核验，勿混用 EXE 与 ZIP 值。

下载 **PCL-Download-Restore-v1a.zip**，完整解压后运行“一键安装启用.cmd”。外置 README 提供最终 ZIP 大小/哈希，发布时复制到正文并附 SHA256SUMS-v1a.txt。自动生成的源码 ZIP 需自行编译。

指定原始 PCL SHA-256：

    6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386

43 项事务、60 项界面/启动检查及真实引擎回归通过。六方法审计及历史运行时结果见 TEST-REPORT.zh-CN.md；本轮未重复联网下载、登录、游戏或 GitHub CI。

**本版未数字签名，NotSigned。** Kuro_Miaow 不是 Windows 已验证发布者。建议购买正版：https://www.minecraft.net/ 。保留上游许可、署名与赞助链接，不捆绑 PCL、游戏或账号。