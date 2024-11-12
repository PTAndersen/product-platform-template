using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Services
{
    public class FileStorageService
    {
        private readonly IFileStorageRepository _fileStorageRepository;

        public FileStorageService(IFileStorageRepository fileStorageRepository)
        {
            _fileStorageRepository = fileStorageRepository;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            return await _fileStorageRepository.UploadFileAsync(file);
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            return await _fileStorageRepository.DeleteFileAsync(fileName);
        }

        public async Task<IEnumerable<string>> ListFilesAsync(string? keyword, int startIndex, int range)
        {
            return await _fileStorageRepository.ListFilesAsync(keyword, startIndex, range);
        }

        public async Task<int> GetFileCountAsync(string? keyword)
        {
            return await _fileStorageRepository.GetFileCountAsync(keyword);
        }
    }
}
