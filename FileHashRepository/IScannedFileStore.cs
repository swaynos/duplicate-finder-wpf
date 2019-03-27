using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public interface IScannedFileStore
    {
        Task<List<string>> ListScannedLocationsAsync();
        Task PurgeLocationsAsync(List<string> locationPaths);
        Task ScanLocationsAsync(List<string> locationPaths, IProgress<int> progress);
        Task RescanLocationsAsync(List<string> locationPaths, IProgress<int> progress);
        Task<List<ScannedFile>> ListDuplicateFilesAsync();
        Task LoadScannedFileStoreFromFileAsync(string filePath);
    }
}
