# GitHub 网页发布教程

[English](PUBLISHING.md) | **简体中文**

目标仓库：https://github.com/maomaox372-commits/PCL-Download-Restore 。

使用已登录的浏览器即可，无需把密码或令牌交给他人。以下为发布材料和操作指引；当前只完成本地准备，本轮尚未向远端上传或发布 v1a。

## 两种交付

- **GitHub 源码**：publication/github/PCL-Download-Restore 内的内容；便携副本为 publication/PCL-Download-Restore-GitHub-Source-v1a.zip，先解压再上传源码。
- **网盘分享**：publication/网盘分享-v1a 内只有 PCL-Download-Restore-v1a.zip 和外置 README.md，两个文件一起上传。

不要上传整个开发目录。干净源码按 source-files.txt 白名单准备，未包含 PCL 本体、游戏、账号、测试下载数据、私钥或本机配置。网页上传不会自动替你按 .gitignore 过滤。

## 1. 建立仓库

打开 https://github.com/new ，填写：

- Repository name：PCL-Download-Restore。
- Description：PCL 2.99.0.0 指定构建的独立第三方下载恢复工具，维护者 Kuro_Miaow，支持原版与整合包入口及备份恢复。
- Visibility：公开源码选 Public；如只想预览可先 Private。
- 不自动生成 README、.gitignore 或其他 license；这些文件已经准备好。

点击 Create repository。随项目保留原 LICENCE 和 LICENSE-STATUS.md，不要在网页选择 MIT/GPL 替换上游文件，不宣传官方背书。

## 2. 上传源码

在空仓库点击 uploading an existing file，或 Add file → Upload files。

打开干净源码目录，拖入**里面全部内容**，保持 src、tests、docs、lib、licenses、.github 等目录层级，不要额外套一层项目目录。确认根目录有 README.md、LICENCE、.gitignore、.gitattributes，且 .github/workflows/build.yml 也上传。

提交说明可写：Release v1a source and documentation。点击 Commit changes，检查首页显示英文 README，语言链接可切换中文。若已经有仓库，更新对应文件并提交即可。

Actions 的 Windows build 编译并运行事务与提示测试；不自动下载 PCL 或游戏，不自动发布。只有网页显示成功才能说 GitHub CI 通过。

## 3. 发布安装包

进入仓库 Releases，选择 Create a new release / Draft a new release：

- Tag：v1a，目标为已上传当前源码的 main 分支。
- Title：PCL 下载恢复助手 v1a（Kuro_Miaow 独立第三方）。
- 说明：复制 docs/releases/v1a.zh-CN.md（英文为 v1a.md）；将网盘外置 README 中最终的 ZIP 大小和 SHA-256 加入说明。
- Assets：上传网盘目录里的 PCL-Download-Restore-v1a.zip 和 README.md，可附 publication/SHA256SUMS-v1a.txt。

可先 Save draft 核对，再 Publish release。说明须保留**未数字签名**状态。GitHub 自动生成的 Source code ZIP 是源码，不是一键安装包。

## 4. 网盘分享

将 publication/网盘分享-v1a 中两个文件一起上传。外置 README 已填入维护者提供的真实仓库地址，请与该仓库的 Releases 发布值交叉核对。

分享文案：PCL 下载恢复助手 v1a，Kuro_Miaow 独立第三方修改。仅支持 README 指定 SHA-256 的 PCL 2.99.0.0；完整解压并运行“一键安装启用.cmd”，确认正版提示后安装。支持备份恢复，本版本未数字签名，请先阅读说明并核对哈希。

## 5. 发布后核对

重新下载附件并检查 SHA-256 与外置 README 一致；查看源码与许可证是否可浏览，README 是否正常渲染。1.0.0 旧包保留不覆盖。以后生产文件有变化时使用新版本号，重新打包并重算哈希。

## 官方流程参考

- https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-new-repository
- https://docs.github.com/en/repositories/working-with-files/managing-files/adding-a-file-to-a-repository
- https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository?tool=webui

按钮位置会随界面更新变化，以上英文为定位关键词。

## 发布校验记录

publication/SHA256SUMS-v1a.txt 列出两个 CMD、配置、EXE、依赖和两个 ZIP 的最终哈希，可作为 Release 附件。EXE 固定值已写进 README 和用户指南；发布时保留“自报哈希不等于可信认证”的提醒。重新编译可能改变 EXE 哈希，需更新文档后重新打包并重算所有哈希，勿覆盖旧同名版本。

首个正式版本为 v1a，内部程序集/文件迭代号保持 1.0.3.0。README.md 默认英文，中文为 README.zh-CN.md / README.zh-HK.md / README.zh-TW.md；配套文档提供英文与简中，许可证原文不替换。不要创建 v1.0.3 或 vv1a 正式标签。
