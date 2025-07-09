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

3. **瀏覽 API 文件**:
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

## 聯絡資訊

如有問題或建議，請透過 GitHub Issues 聯絡我們。
