namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IFileStorageRepository
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<bool> DeleteFileAsync(string fileName);
        Task<IEnumerable<string>> ListFilesAsync(string? keyword, int startIndex, int range);
        Task<int> GetFileCountAsync(string? keyword);
    }
}
