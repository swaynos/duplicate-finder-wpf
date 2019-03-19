using FileHashRepository.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public class ScannedFileStore : IScannedFileStore
    {
        private IFileHashService _service;

        private IFileSystem _fileSystem;

        /// <summary>
        /// Internally exposed for testing
        /// </summary>
        /// <param name="fileSystem">The IFileSystem to use</param>
        /// <param name="fileHashServiceFactory">The IFileHashServiceFactory to use</param>
        internal ScannedFileStore(IFileSystem fileSystem, IFileHashService service)
        {
            _service = service;
            _fileSystem = fileSystem;
        }

        public ScannedFileStore(IFileHashService service) : this(new FileSystem(), service)
        {
        }

        /// <summary>
        /// List all previously scanned locations
        /// </summary>
        /// <returns>A collection of all previously scanned locations</returns>
        public async Task<List<string>> ListScannedLocationsAsync()
        {
             return await _service.ListScannedLocationsAsync();
        }

        /// <summary>
        /// This will purge all existing records from the storage backend first at the given
        /// location paths.
        /// </summary>
        /// <param name="locationPaths">The locations to purge scanned file records from</param>
        public async Task PurgeLocationsAsync(List<string> locationPaths)
        {
            // Purge all existing records from storage at these location paths
            await _service.PurgeScannedLocationsAsync(locationPaths);
        }

        /// <summary>
        /// Scan all files from location and calculates the SHA256 hash for each file.
        /// These records are placed on the storage backend for later query.
        /// </summary>
        /// <param name="locationPath">The path to scan</param>
        public async Task ScanLocationsAsync(List<string> locationPaths, IProgress<int> progress)
        {
            // Purge all existing records from storage at these location paths
            await _service.PurgeScannedLocationsAsync(locationPaths);
            
            // Get all files from the location path
            List<string> files = new List<string>();
            foreach (string locationPath in locationPaths)
            {
                files.AddRange(await GetFilesAsync(locationPath, "*.*", SearchOption.AllDirectories));
            }

            int totalCount = files.Count;
            for (int i = 0; i < files.Count; i++)
            {
                string filePath = files[i];
                await ScanFile(filePath, progress, i+1, totalCount);
            }

            // Insert the scanned locations
            foreach (string locationPath in locationPaths)
            {
                await _service.InsertScannedLocationAsync(new ScannedLocation()
                {
                    Path = locationPath
                });
            }
        }

        /// <summary>
        /// Rescans the locations and determines if any files were added or removed. 
        /// Inserting new records on the storage backend, or purging scanned file records, respectively.
        /// <b>Assumes</b> that files existing at the same path have not been modified
        /// </summary>
        /// <param name="locationPath">The path to scan</param>
        public async Task RescanLocationsAsync(List<string> locationPaths, IProgress<int> progress)
        {
            FileHash fileHash = new FileHash(_fileSystem);

            List<string> storedFiles = new List<string>();
            List<string> files = new List<string>();

            // Async delegates which we will wait on
            Func<Task> retrieveStoredFiles, retrieveFiles;

            // Retrieve all existing scanned file records at these locations from storage
            retrieveStoredFiles = async () =>
            {
                storedFiles = await _service.ListScannedFilePathsAsync(locationPaths);
            };

            retrieveFiles = async () =>
            {
                foreach (string locationPath in locationPaths)
                {
                    files.AddRange(await GetFilesAsync(locationPath, "*.*", SearchOption.AllDirectories));
                }
            };

            // Invoke and wait for our retrieval delegates to finish before continuing on this thread
            await Task.WhenAll(retrieveStoredFiles.Invoke(), retrieveFiles.Invoke());

            // Determine all files that are not in storedFiles, these are additions
            IEnumerable<string> additions = files.Except(storedFiles);
            // Determine all files that are not in storedFiles, these are removals
            IEnumerable<string> removals = storedFiles.Except(files);
            // This total count is what we will update progress on
            int totalCount = additions.Count() + removals.Count();
            int tempCount = 0;

            foreach (string addition in additions)
            {
                await ScanFile(addition, progress, ++tempCount, totalCount);
            }

            foreach (string removal in removals)
            {
                await RemoveFile(removal, progress, ++tempCount, totalCount);
            }

            // Insert the scanned locations
            foreach (string locationPath in locationPaths)
            {
                await _service.InsertScannedLocationAsync(new ScannedLocation()
                {
                    Path = locationPath
                });
            }
        }

        /// <summary>
        /// Helper method which wraps _fileSystem.Directory.GetFiles() in an async Task
        /// </summary>
        private async Task<string[]> GetFilesAsync(string locationPath, string searchPattern, SearchOption searchOption)
        {
            Task<string[]> task = Task.Run(() =>
            {
               return _fileSystem.Directory.GetFiles(locationPath, searchPattern, searchOption);
            });
            return await task;

            // The only way we reach here is if an exception occured
            throw task.Exception;
        }

        /// <summary>
        /// Returns all scanned files which have at least one duplicate.
        /// </summary>
        /// <returns>The List of ScannedFile entities with duplicates.</returns>
        public async Task<List<ScannedFile>> ListDuplicateFilesAsync()
        {
            return await _service.ReturnDuplicatesAsync();
        }

        /// <summary>
        /// Scan a file to the storage backend and update the progress
        /// </summary>
        /// <param name="filePath">The location to add to the storage backend</param>
        /// <param name="progress">The IProgress to update the progress on. 
        /// Progress is updated with the following formula index * 100 / totalCount</param>
        internal async Task ScanFile(string filePath, IProgress<int> progress, int index, int totalCount)
        {
            if (_fileSystem.File.Exists(filePath))
            {
                FileHash fileHash = new FileHash(_fileSystem);
                ScannedFile scannedFile = new ScannedFile();
                scannedFile.Name = _fileSystem.Path.GetFileName(filePath);
                scannedFile.Path = _fileSystem.Path.GetFullPath(filePath);
                scannedFile.Hash = await fileHash.ComputeFileHashAsync(filePath);
                await _service.InsertScannedFileAsync(scannedFile);
            }
            if (progress != null)
            {
                // This is not an atomic operation (++tempCound and Report)
                // To ensure seamless Reports we need to mutex lock
                progress.Report(index * 100 / totalCount);
            }
        }

        /// <summary>
        /// Remove a file from the storage backend and update the progress
        /// </summary>
        /// <param name="filePath">The location to add to the storage backend</param>
        /// <param name="progress">The IProgress to update the progress on. 
        /// Progress is updated with the following formula index * 100 / totalCount</param>
        internal async Task RemoveFile(string filePath, IProgress<int> progress, int index, int totalCount)
        {
            await _service.RemoveScannedFilesByFilePathAsync(filePath);

            if (progress != null)
            {
                progress.Report(index * 100 / totalCount);
            }
        }
    }
}
