namespace PdfMergeApi.Services
{
    public interface IPdfService
    {
        byte[] MergePdfsInDirectory(string directoryPath);
    }
}
