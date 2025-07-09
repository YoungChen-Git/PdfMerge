# 使用官方的 .NET 9.0 SDK 作為建置階段
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# 設定工作目錄
WORKDIR /app

# 複製 solution 檔案和專案檔案
COPY *.sln .
COPY pdfmerge/*.csproj ./pdfmerge/

# 還原 NuGet 套件
RUN dotnet restore

# 複製所有原始碼
COPY . .

# 建置應用程式
WORKDIR /app/pdfmerge
RUN dotnet publish -c Release -o out

# 使用官方的 .NET 9.0 Runtime 作為執行階段
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# 安裝必要的字型和套件 (PdfSharp 可能需要)
RUN apt-get update && apt-get install -y \
    fontconfig \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

# 設定工作目錄
WORKDIR /app

# 複製建置後的檔案
COPY --from=build /app/pdfmerge/out .

# 建立一個目錄來存放要合併的 PDF 檔案
RUN mkdir -p /app/uploads

# 設定環境變數
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# 暴露端口
EXPOSE 8080

# 設定啟動命令
ENTRYPOINT ["dotnet", "pdfmerge.dll"]
