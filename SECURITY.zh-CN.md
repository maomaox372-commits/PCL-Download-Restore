# 安全问题

[English](SECURITY.md) | **简体中文**

本工具修改目标 EXE，哈希、路径、备份和恢复完整性是关键边界。当前仅维护 README 所列精确构建。

不要公开上传密钥、账号配置或私密日志。若本仓库开启 Private vulnerability reporting，请用 Security → Report a vulnerability；未开启时只发不含利用细节的联系请求，由维护者建立私密渠道。未声明尚未配置的邮箱或响应时限。

提供最小复现、版本、预期/实际行为和备份状态，不用真实游戏目录做破坏性测试。安全提示说明见 docs/SECURITY-WARNINGS.md；未签名、未报毒或一次扫描通过均不能单独证明安全。

更新选项默认不禁用官方更新。主动勾选可能错过官方安全修复或回滚，界面始终保留红色警告；解除与手动修改详见 docs/UPDATE-OPTIONS.md。发布新构建须重算 EXE 和 ZIP 哈希，不得沿用旧值。
