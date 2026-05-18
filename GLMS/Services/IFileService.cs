namespace GLMS.Services
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string subDirectory);
        bool IsValidFile(IFormFile file);
        bool IsValidFileType(IFormFile file);
        Task<bool> DeleteFileAsync(string filePath);
    }
}