using Microsoft.AspNetCore.Mvc;
using PdfMergeApi.Models;
using PdfMergeApi.Services;

namespace PdfMergeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly IPdfService _pdfService;

        public PdfController(IPdfService pdfService)
        {
            _pdfService = pdfService;
        }

        [HttpPost("merge")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult MergePdfs([FromBody] MergeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DirectoryPath))
            {
                return BadRequest("DirectoryPath 欄位不可為空。");
            }

            try
            {
                byte[] mergedPdfBytes = _pdfService.MergePdfsInDirectory(request.DirectoryPath);
                string outputFileName = $"merged_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                
                // 將 byte array 作為檔案回傳
                return File(mergedPdfBytes, "application/pdf", outputFileName);
            }
            catch (DirectoryNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // 建議在此處記錄詳細的錯誤日誌
                return StatusCode(500, $"處理過程中發生內部錯誤: {ex.Message}");
            }
        }
    }
}