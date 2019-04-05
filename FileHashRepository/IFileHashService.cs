using FileHashRepository.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public interface IFileHashService
    {
        Task InsertScannedFileAsync(ScannedFile scannedFile);
        Task InsertScannedLocationAsync(ScannedLocation scannedLocation);
        Task<List<string>> ListScannedFilePathsAsync(List<string> locationPaths);
        Task<List<string>> ListScannedLocationsAsync();
        Task PurgeScannedLocationsAsync(List<string> locationPaths);
        Task<int> RemoveScannedFilesByFilePathAsync(string filePath);
        Task<List<ScannedFile>> ReturnDuplicatesAsync();
        void UpdateDataCaches(IDataCache<ScannedFile> files, IDataCache<ScannedLocation> locations);
        IEnumerable<ScannedLocation> ListScannedLocations();
        IEnumerable<ScannedFile> ListScannedFiles();
    }
}
