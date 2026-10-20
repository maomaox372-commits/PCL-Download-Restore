# PCL 下載還原助手 v1a（獨立第三方）

**PCL 原作者：龍騰貓躍（龙腾猫跃）· [支持原作者](https://afdian.com/a/LTCat) · [官方儲存庫](https://github.com/Meloong-Git/PCL)**

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文（香港）](README.zh-HK.md) | **繁體中文（台灣）**

**正式發布版本：v1a（第一版）**。維護者：**Kuro_Miaow**。正式版本編號以實際正式發布為準；之前的數字版本均為內部除錯迭代。

本項目是第三方基於 PCL 獨立進行二次創作的成果，為 **PCL 2.99.0.0 的一個指定組建**復原原版 Minecraft 及模組整合包下載入口，並非官方外掛程式，亦不代表原作者。沿用 PCL 原有下載器、元件安裝、執行個體隔離及帳號系統，不附帶官方或修改版 PCL EXE、遊戲、模組或帳號資料。桌面介面仍為簡體中文；多語言版本只適用於文件。

## 支持正版及原作者

**建議透過 [Minecraft 官方渠道](https://www.minecraft.net/) 購買正版遊戲，亦歡迎[支持 PCL 原作者](https://afdian.com/a/LTCat)。** 本工具不提供遊戲授權。每次啟動助手（包括 CLI 及 --help）均會顯示只有 **确定(Y)** 一個按鈕的提示，可按一下或按 Y、Alt+Y、Enter。沒有略過或不再提示選項；確認只表示已閱讀。

**官方更新或回復舊版本可直接覆寫本修補程式，助手預設不停用更新。日後 PCL2 更新後，可能需要安裝新版指令碼／工具，或在相容時重新執行目前版本以啟用。只支援已核實的組建，不相容時會拒絕修補；不會阻止官方覆寫，亦不會自動重新套用修補程式。**

修補後的 PCL 主視窗載入時亦會顯示「Kuro_Miaow 第三方修改」，清楚標示非官方身分。

## 下載及使用

在儲存庫 **Releases → Assets** 下載 **PCL-Download-Restore-v1a.zip**。GitHub 自動產生的 Source code ZIP 及 Code → Download ZIP 是原始碼，須自行編譯。雲端硬碟分享為同一安裝 ZIP 加獨立簡中 README。

1. 關閉 PCL，完整解壓縮；不要在壓縮軟體預覽內執行。
2. 連按兩下 **一键安装启用.cmd**，閱讀提示並確認。未找到相容檔案時選取自己的 PCL EXE，亦可把 EXE 拖放到 CMD。
3. 完成後自行啟動 PCL，使用原版自動安裝、線上整合包或本機匯入功能。

**管理与恢复.cmd** 提供選項及還原。「禁用 PCL 自动更新」預設不勾選；勾選代表「是」，紅色警告會說明可能錯過官方修正及回復舊版本。只有按 **安装并启用** 後才生效，方格並非目前安裝狀態偵測。只有主動勾選或 --lock-updates 才會寫入 SystemSystemUpdate:3。

解除工具施加的更新鎖：關閉 PCL，保留原路徑及 .pcl-download-restore，取消勾選後再次安裝。還原／安裝流程會移除已記錄的鎖，保留使用者後來的選擇；不會強制啟用使用者原先已關閉的更新，亦不會覆寫未知官方更新檔案。詳見[更新及手動修改說明（英文）](docs/UPDATE-OPTIONS.md)／[簡中](docs/UPDATE-OPTIONS.zh-CN.md)。

管理介面採用藍色標題與按鈕、淺藍背景、白色卡片及紅色警告。需要 Windows、.NET Framework 4.8 及目標資料夾寫入權限；使用時不需要 Python、Node.js 或管理員權限。見[使用指南（英文）](docs/USER-GUIDE.md)／[簡中](docs/USER-GUIDE.zh-CN.md)。

## 執行前核對完整性

發布儲存庫：https://github.com/maomaox372-commits/PCL-Download-Restore 。請與獨立取得的**相同版本、相同檔案**發布記錄核對。

**v1a PCL.DownloadRestore.exe SHA-256：**

    FFCAA53E99BE9DD56E74A8FB67ACCE66C79E14E3EBFE7DD03F710BEE72BF9AF3

**雜湊值不符時，請立即停止使用並刪除檔案，再從儲存庫重新取得；切勿繼續執行。**

    Get-FileHash -LiteralPath './PCL.DownloadRestore.exe' -Algorithm SHA256
    Get-FileHash -LiteralPath './PCL-Download-Restore-v1a.zip' -Algorithm SHA256

啟動視窗會計算目前 EXE 的雜湊值。**程式自行顯示的值不能證明其可信**，必須在執行前與獨立發布值核對。EXE 與 ZIP 的值不同，自行編譯亦可能改變值。包內 SHA256SUMS.txt 列出逐檔數值；ZIP 最終值見外置 README 或發布清單。

**本版未經數位簽章，狀態為 NotSigned。** Kuro_Miaow 是署名，並非 Windows 已驗證的發行者。系統仍可能顯示安全警告或 SmartScreen。本工具不安裝信任憑證；不要停用安全軟體或修改全域信任原則。見[安全提示（英文）](docs/SECURITY-WARNINGS.md)／[簡中](docs/SECURITY-WARNINGS.zh-CN.md)。

## 指定相容性及範圍

只接受下列原始 PCL EXE SHA-256：

    6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386

即使版本編號相同，指紋不同亦不支援。復原原版清單及既有 Forge／NeoForge／Fabric 選項、線上整合包安裝與另存、本機 CurseForge／Modrinth／HMCL／MMC／MCBBS 等 PCL 原本可辨識的壓縮檔，以及已安裝執行個體的 Minecraft 卡片。一般模組資源沿用原有儲存行為。

檔案下架、平台限制、網路故障、損壞資訊清單及不相容元件仍可導致失敗，不能保證所有整合包成功。不修改帳號、購買提示、贊助權限、PCL 佈景主題、憑證或安全軟體。

## 備份及還原

保留目標旁的 .pcl-download-restore。操作會核對雜湊及路徑、拒絕處理執行中的目標，並採用同目標互斥、暫存、不可分割取代及失敗還原。保留後來更改的其他設定及更新值；未知或遭竄改的檔案會被拒絕覆寫。

記錄綁定原完整路徑：**移動或重新命名前先還原，之後再安裝。** 預設安裝不會建立原先不存在的 Setup.ini；只因更新鎖建立的設定還原後保留空檔，不會刪除使用者路徑。

## 驗證及開發

實際檢查及限制見[測試報告（英文）](TEST-REPORT.md)／[簡中](TEST-REPORT.zh-CN.md)。歷史隔離測試完成 Minecraft 1.20.1 及非空 Modrinth／CurseForge 測試包下載。本輪文件與改名不重做網路下載、登入或遊戲執行。

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./build.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./test.ps1
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./tests/StartupNoticeTests.ps1

使用系統 .NET Framework C# 編譯器，隨附固定 Mono.Cecil 0.11.6 net40 DLL，不需要線上 NuGet。組件／檔案版本 **1.0.3.0** 僅供內部除錯；對外 ProductVersion 及標籤為 **v1a**。重新編譯後須更新文件雜湊；封裝器會拒絕舊 EXE 雜湊及已存在的同名 ZIP。

CLI：--quick-install [PCL.exe]、--install <PCL.exe> [--lock-updates]、--restore <PCL.exe>、--inspect <PCL.exe>，相容 --no-update-lock。所有模式須確認提示，不支援無人值守；成功回傳 0，失敗非 0，一鍵選取取消為 2。

## 來源、授權及回報

原樣保留並遵循上游 [LICENCE](LICENCE)，並非整個項目的 MIT／GPL 授權；見[授權記錄](LICENSE-STATUS.md)及[署名](NOTICE.md)，兩者均有簡中切換。Mono.Cecil 另適用 [MIT](licenses/Mono.Cecil-LICENSE.txt)，不聲稱額外官方授權或認可。

問題請向本儲存庫提供已移除私隱資料的步驟、版本及雜湊；不要上傳帳號、權杖、存檔或整套 PCL。見[貢獻](CONTRIBUTING.md)、[安全](SECURITY.md)、[發布](docs/PUBLISHING.md)、[v1a 說明](docs/releases/v1a.md)、[技術](docs/TECHNICAL.md)及[執行時重現](tests/runtime-reproduce.md)。配套文件提供英文及簡中。