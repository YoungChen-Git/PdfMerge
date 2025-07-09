# 這個目錄用於存放要合併的 PDF 檔案

將您要合併的 PDF 檔案放在這個目錄中，服務會自動掃描這個目錄及其子目錄中的所有 PDF 檔案並進行合併。

## 使用方式

1. 將 PDF 檔案放入此目錄
2. 呼叫 API: `POST /api/pdf/merge`
3. 請求內容: `{"directoryPath": "/app/uploads"}`
