# PDF 合併服務 (PDF Merge Service)

這是一個基於 ASP.NET Core 的 Web API 服務，可以將指定目錄中的所有 PDF 文件合併成一個單一的 PDF 文件。

## 功能特色

- 🔗 **PDF 合併**: 將指定目錄及其子目錄中的所有 PDF 文件合併成一個文件
- 📁 **遞迴搜尋**: 自動搜尋所有子目錄中的 PDF 文件
- 🚀 **RESTful API**: 提供簡潔的 HTTP API 介面
- 🐳 **Docker 支援**: 可輕鬆部署至任何支援 Docker 的環境
- 📄 **OpenAPI/Swagger**: 內建 API 文件

## 技術規格

- **.NET 9.0**: 使用最新的 .NET 技術
- **PdfSharp**: 強大的 PDF 處理庫
- **ASP.NET Core**: 高效能的 Web 框架
- **xUnit**: 單元測試框架
- **FluentAssertions**: 流暢的斷言庫
- **Moq**: 模擬框架

## 測試

本專案包含完整的單元測試和整合測試，確保程式碼品質和可靠性。

### 測試覆蓋範圍

- 🧪 **單元測試**: `PdfService` 和 `PdfController` 的所有主要功能
- 🔗 **整合測試**: 完整的 API 端點測試
- 📝 **模型測試**: 資料模型的驗證測試

### 執行測試

**使用腳本 (推薦)**:
```bash
# Linux/macOS
chmod +x run-tests.sh
./run-tests.sh

# Windows
run-tests.bat
```

**手動執行**:
```bash
# 執行所有測試
dotnet test

# 執行測試並生成覆蓋率報告
dotnet test --collect:"XPlat Code Coverage"

# 只執行單元測試
dotnet test --filter "FullyQualifiedName!~Integration"

# 只執行整合測試
dotnet test --filter "FullyQualifiedName~Integration"
```

### 測試結構

```
pdfmerge.Tests/
├── Controllers/
│   └── PdfControllerTests.cs       # Controller 單元測試
├── Services/
│   └── PdfServiceTests.cs          # Service 單元測試
├── Models/
│   └── MergeRequestTests.cs        # Model 測試
├── Integration/
│   └── PdfControllerIntegrationTests.cs  # 整合測試
└── GlobalUsings.cs                 # 全域 using 宣告
```

## 快速開始

### 使用 Docker (推薦)

1. **建置 Docker 映像檔**:
   ```bash
   docker build -t pdf-merge-service .
   ```

2. **執行容器**:
   ```bash
   docker run -p 8080:8080 -v /path/to/your/pdfs:/app/uploads pdf-merge-service
   ```

### 本地開發

1. **安裝必要條件**:
   - [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

2. **還原套件並執行**:
   ```bash
   cd pdfmerge
   dotnet restore
   dotnet run
   ```

3. **執行測試**:
   ```bash
   # 快速執行所有測試
   ./run-tests.sh
   
   # 或手動執行
   dotnet test
   
   # 期望結果: 30/30 測試通過 ✅
   ```

4. **瀏覽 API 文件**:
   - 開發模式: `https://localhost:7135/openapi`

## API 使用方式

### 合併 PDF 文件

**端點**: `POST /api/pdf/merge`

**請求內容**:
```json
{
  "directoryPath": "/path/to/pdf/directory"
}
```

**回應**: 
- 成功時返回合併後的 PDF 文件 (application/pdf)
- 文件名格式: `merged_YYYYMMDDHHMMSS.pdf`

**錯誤回應**:
- `400 Bad Request`: DirectoryPath 欄位為空
- `404 Not Found`: 指定的目錄不存在
- `500 Internal Server Error`: 處理過程中發生錯誤

### 使用範例

#### cURL
```bash
curl -X POST "http://localhost:8080/api/pdf/merge" \
     -H "Content-Type: application/json" \
     -d '{"directoryPath":"/app/uploads"}' \
     -o merged_output.pdf
```

#### JavaScript (Fetch API)
```javascript
const response = await fetch('http://localhost:8080/api/pdf/merge', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    directoryPath: '/app/uploads'
  })
});

if (response.ok) {
  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'merged.pdf';
  a.click();
}
```

## 部署選項

### Docker 部署

1. **本地部署**:
   ```bash
   # 建置映像檔
   docker build -t pdf-merge-service .
   
   # 執行服務
   docker run -d -p 8080:8080 --name pdf-merge pdf-merge-service
   ```

2. **使用 Docker Compose**:
   ```yaml
   version: '3.8'
   services:
     pdf-merge:
       build: .
       ports:
         - "8080:8080"
       volumes:
         - ./uploads:/app/uploads
       environment:
         - ASPNETCORE_ENVIRONMENT=Production
   ```

**注意**: Docker 建置過程中會自動執行所有測試，確保部署的程式碼品質。

### 持續整合 (CI/CD)

建議在 CI/CD 管道中包含以下步驟：

```bash
# 1. 還原依賴項
dotnet restore

# 2. 建置專案
dotnet build --no-restore

# 3. 執行測試
dotnet test --no-build --verbosity normal

# 4. 發布應用程式
dotnet publish -c Release -o ./publish
```

### 雲端部署

#### Azure Container Instances
```bash
az container create \
  --resource-group myResourceGroup \
  --name pdf-merge-service \
  --image pdf-merge-service \
  --ports 8080 \
  --environment-variables ASPNETCORE_ENVIRONMENT=Production
```

#### Google Cloud Run
```bash
# 推送到 Google Container Registry
docker tag pdf-merge-service gcr.io/[PROJECT-ID]/pdf-merge-service
docker push gcr.io/[PROJECT-ID]/pdf-merge-service

# 部署到 Cloud Run
gcloud run deploy pdf-merge-service \
  --image gcr.io/[PROJECT-ID]/pdf-merge-service \
  --platform managed \
  --port 8080
```

#### AWS Fargate
使用 AWS ECS 或通過 AWS App Runner 部署。

## 環境變數

| 變數名稱 | 預設值 | 說明 |
|---------|--------|------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | 應用程式環境 |
| `ASPNETCORE_URLS` | `http://+:8080` | 監聽的 URL |

## 安全性考量

⚠️ **重要安全提醒**:

1. **路徑安全**: 目前版本沒有對輸入路徑進行安全驗證，在生產環境中應該:
   - 限制可存取的目錄範圍
   - 驗證路徑以防止目錄遍歷攻擊
   - 實作檔案大小限制

2. **建議的安全性改進**:
   ```csharp
   // 在 PdfService 中加入路徑驗證
   private bool IsPathSafe(string path)
   {
       var allowedBasePath = "/app/uploads";
       var fullPath = Path.GetFullPath(path);
       return fullPath.StartsWith(Path.GetFullPath(allowedBasePath));
   }
   ```

## 故障排除

### 常見問題

1. **容器無法啟動**:
   - 檢查端口是否被佔用
   - 確認 Docker 版本支援 .NET 9.0

2. **PDF 合併失敗**:
   - 確認 PDF 文件沒有密碼保護
   - 檢查文件權限
   - 查看容器日誌: `docker logs <container-id>`

3. **文件找不到**:
   - 確認目錄路徑正確
   - 檢查 Volume 掛載是否正確

### 日誌查看

```bash
# 查看容器日誌
docker logs pdf-merge-service

# 即時監控日誌
docker logs -f pdf-merge-service
```

## 授權

本專案採用 MIT 授權條款。

## 貢獻

歡迎提交 Issue 和 Pull Request！

在提交 PR 之前，請確保：
- 所有測試通過：`dotnet test`
- 程式碼遵循現有的風格
- 新功能包含適當的測試

## 測試報告

詳細的測試覆蓋範圍和測試案例，請參考 [測試報告](TEST_REPORT.md)。

## 聯絡資訊

如有問題或建議，請透過 GitHub Issues 聯絡我們。
