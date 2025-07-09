using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Diagnostics;
using System.Security;

namespace PdfMergeApi.Services
{
    public class PdfService : IPdfService
    {
        /// <summary>
        /// 在指定目錄及其子目錄中尋找所有 PDF 檔案並將它們合併成一個檔案。
        /// </summary>
        /// <param name="directoryPath">要搜尋的根目錄路徑。</param>
        /// <returns>包含合併後 PDF 內容的位元組陣列。</returns>
        /// <exception cref="DirectoryNotFoundException">當指定的目錄不存在時拋出。</exception>
        /// <exception cref="FileNotFoundException">當目錄中找不到任何 PDF 檔案時拋出。</exception>
        public byte[] MergePdfsInDirectory(string directoryPath)
        {
            // **安全性注意**: 在生產環境中，您應該驗證此路徑是否在一個允許的、安全的基底目錄下，
            // 以防止目錄遍歷攻擊 (Path Traversal)。
            // 例如: if (!IsPathSafe(directoryPath)) throw new SecurityException("...");

            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"指定的目錄不存在: {directoryPath}");
            }

            // 使用 SearchOption.AllDirectories 來遞迴搜尋所有子資料夾
            string[] pdfFiles = Directory.GetFiles(directoryPath, "*.pdf", SearchOption.AllDirectories);

            if (pdfFiles.Length == 0)
            {
                throw new FileNotFoundException("在指定的目錄或其子目錄中找不到任何 PDF 檔案。");
            }

            // 建立一個新的 PDF 文件用於輸出
            using (var outputDocument = new PdfDocument())
            {
                foreach (string pdfFile in pdfFiles)
                {
                    try
                    {
                        // 以匯入模式開啟每個輸入的 PDF 文件
                        using (var inputDocument = PdfReader.Open(pdfFile, PdfDocumentOpenMode.Import))
                        {
                            // 將來源文件的每一頁加入到輸出文件中
                            foreach (PdfPage page in inputDocument.Pages)
                            {
                                outputDocument.AddPage(page);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 如果某個 PDF 檔案損毀或無法讀取，紀錄錯誤並繼續處理下一個檔案
                        Debug.WriteLine($"無法處理檔案 {pdfFile}: {ex.Message}");
                    }
                }

                // 將合併後的文件儲存到記憶體流中
                using var stream = new MemoryStream();
                outputDocument.Save(stream, false);
                return stream.ToArray();
            }
        }
    }
}