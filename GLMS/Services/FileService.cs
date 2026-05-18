namespace GLMS.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly string[] _allowedExtensions = { ".pdf" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
        }

        public bool IsValidFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                return false;
            }

            return IsValidFileType(file);
        }

        public bool IsValidFileType(IFormFile? file)
        {
            if (file == null) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var isValidExtension = _allowedExtensions.Contains(extension);
            var isValidContentType = file.ContentType == "application/pdf";

            return isValidExtension && isValidContentType;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string subDirectory)
        {
            if (!IsValidFile(file))
            {
                throw new InvalidOperationException("Invalid file. Only PDF files up to 10MB are allowed.");
            }

            try
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subDirectory);

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"File uploaded successfully: {uniqueFileName}");
                return $"/uploads/{subDirectory}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation($"File deleted successfully: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
                return false;
            }
        }
    }
}