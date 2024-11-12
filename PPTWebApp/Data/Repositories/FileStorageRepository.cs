using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Repositories
{
    public class FileStorageRepository : IFileStorageRepository
    {
        private readonly string _storagePath;

        public FileStorageRepository(string storagePath)
        {
            _storagePath = storagePath;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var filePath = Path.Combine(_storagePath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);

            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string? keyword, int startIndex, int range)
        {
            var files = Directory.GetFiles(_storagePath)
                                 .Select(Path.GetFileName);

            if (!string.IsNullOrEmpty(keyword))
            {
                files = files.Where(f => f.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            files = files.Skip(startIndex).Take(range);

            return await Task.FromResult(files);
        }

        public async Task<int> GetFileCountAsync(string? keyword)
        {
            var files = Directory.GetFiles(_storagePath)
                                 .Select(Path.GetFileName);

            if (!string.IsNullOrEmpty(keyword))
            {
                files = files.Where(f => f.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            return await Task.FromResult(files.Count());
        }
    }
}
