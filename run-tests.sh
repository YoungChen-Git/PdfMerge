#!/bin/bash

# 測試腳本 - 執行所有測試

echo "🧪 開始執行 PDF 合併服務的單元測試..."

# 切換到測試專案目錄
cd pdfmerge.Tests

# 還原套件
echo "📦 還原 NuGet 套件..."
dotnet restore

# 執行測試
echo "🔍 執行測試..."
dotnet test --verbosity normal --collect:"XPlat Code Coverage"

# 檢查測試結果
if [ $? -eq 0 ]; then
    echo "✅ 所有測試通過！"
else
    echo "❌ 測試失敗！"
    exit 1
fi

echo "📊 測試報告已生成，請查看 TestResults 目錄。"
