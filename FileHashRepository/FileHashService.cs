using FileHashRepository.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public class FileHashService : IFileHashService
    {
        private IDataCache<ScannedFile> _scannedFiles;

        private IDataCache<ScannedLocation> _scannedLocations;

        /// <summary>
        /// Create a new FileHashService with provided data caches.
        /// </summary>
        /// <param name="scannedFiles">The data cache of scanned files to use</param>
        /// <param name="scannedLocations">The data cache of scanned locations to use</param>
        public FileHashService(IDataCache<ScannedFile> scannedFiles, IDataCache<ScannedLocation> scannedLocations)
        {
            _scannedFiles = scannedFiles;
            _scannedLocations = scannedLocations;
        }

        /// <summary>
        /// Inserts a ScannedFile into the storage backend
        /// </summary>
        /// <param name="scannedFile">The ScannedFile to insert</param>
        public async Task InsertScannedFileAsync(ScannedFile scannedFile)
        {
            if(!await ScannedFilesContains(scannedFile))
            {
                _scannedFiles.InsertData(scannedFile);
            }
        }

        /// <summary>
        /// Inserts a ScannedLocation into the storage backend
        /// </summary>
        /// <param name="scannedLocation">The ScannedLocation to insert</param>
        public async Task InsertScannedLocationAsync(ScannedLocation scannedLocation)
        {
            if (!await ScannedLocationsContains(scannedLocation))
            {
                _scannedLocations.InsertData(scannedLocation);
            }
        }

        /// <summary>
        /// Provides the list of ScannedFile.Path's filtered by the locationPaths
        /// </summary>
        /// <param name="locationPaths">Optional: The location to return scanned files from.
        /// <para>If null everything will be returned.</para>
        /// <para>If empty, an empty List will be returned.</para>
        /// </param>
        /// <returns>A List of strings which contain the ScannedFile.Path's</returns>
        public async Task<List<string>> ListScannedFilePathsAsync(List<string> locationPaths)
        {
            // ToDo: Not an async method
            List<string> scannedFilePaths;

            if (locationPaths == null)
            {
                scannedFilePaths =  _scannedFiles.ListData().Select(t => t.Path).ToList();
            }
            else if (locationPaths.Count == 0)
            {
                scannedFilePaths = new List<string>();
            }
            else
            {
                // ToDo: Do we want a full string compare, or begins with?
                // Is this the best LINQ query possible?
                scannedFilePaths =  _scannedFiles.ListData()
                    .Where(t => locationPaths.Contains(t.Path))
                    .Select(t => t.Path)
                    .ToList();
            }

            return scannedFilePaths;
        }

        /// <summary>
        /// Provides the list of all ScannedLocation.Path's
        /// </summary>
        /// <returns>A List of strings which contain the ScannedLocation.Path's</returns>
        public async Task<List<string>> ListScannedLocationsAsync()
        {
            // ToDo: Not an async method
            return _scannedLocations.ListData().Select(t => t.Path).ToList();
        }

        /// <summary>
        /// Removes all ScannedFiles from the storage Backend at the given location
        /// </summary>
        /// <param name="locationPaths">Optional: The location to purge scanned files from.
        /// <para>If null everything will be purged.</para>
        /// <para>If empty, nothing will be purged.</para></param>
        public async Task PurgeScannedLocationsAsync(List<string> locationPaths)
        {
            // ToDo: No longer async
            if (locationPaths == null)
            {
                _scannedFiles.PurgeData(_scannedFiles.ListData());
            }
            else
            {
                // ToDo: Do we want a full string compare, or begins with?
                // Is this the best LINQ query possible?
                IQueryable<ScannedLocation> scannedLocations = _scannedLocations.ListData().Where(t => locationPaths.Contains(t.Path));
                _scannedLocations.PurgeData(scannedLocations);
            }
        }

        /// <summary>
        /// Removes all ScannedFile records from the storage backend by a file path
        /// </summary>
        /// <param name="filePath">The file path of the file record(s)</param>
        /// <returns>The number of ScannedFiles removed</returns>
        public async Task<int> RemoveScannedFilesByFilePathAsync(string filePath)
        {
            // ToDo: Not an async method
            IQueryable<ScannedFile> scannedFiles = _scannedFiles.ListData().Where(t => t.Path.Equals(filePath));
            int removedRecords =  scannedFiles.Count(); // ToDo: This is unnecessary
            _scannedFiles.PurgeData(scannedFiles);
            return removedRecords;
        }

        /// <summary>
        /// Returns all ScannedFile entities who have a Hash that is present in the storage backend
        /// more than once.
        /// </summary>
        /// <returns>A list of ScannedFile entities</returns>
        public async Task<List<ScannedFile>> ReturnDuplicatesAsync()
        {
            // ToDo: Not an async method
            IQueryable<ScannedFile> scannedFiles = 
                _scannedFiles.ListData()
                    .GroupBy(t => t.Hash)
                    .Where(t => t.Count() > 1)
                    .SelectMany(group => group);
            return scannedFiles.ToList();
        }

        /// <summary>
        /// Iterate through the ScannedFiles to determine if there is an equal ScannedFile
        /// </summary>
        /// <param name="scannedFile">The ScannedFile to compare against the ScannedFiles in the dbContext</param>
        /// <returns>true/false if the ScannedFile is contained in the dbContext</returns>
        private async Task<bool> ScannedFilesContains(ScannedFile scannedFile)
        {
            Task<bool> task = Task.Run(() =>
            {
                bool contains = false;
                foreach (ScannedFile contextScannedFile in _scannedFiles.ListData())
                {
                    contains |= contextScannedFile.Equals(scannedFile);
                }
                return contains;
            });
            
            return await task;
        }

        /// <summary>
        /// Iterate through the ScannedFiles to determine if there is an equal ScannedFile
        /// </summary>
        /// <param name="scannedFile">The ScannedFile to compare against the ScannedFiles in the dbContext</param>
        /// <returns>true/false if the ScannedFile is contained in the dbContext</returns>
        private async Task<bool> ScannedLocationsContains(ScannedLocation scannedLocation)
        {
            Task<bool> task = Task.Run(() =>
            {
                bool contains = false;
                foreach (ScannedLocation contextScannedLocation in _scannedLocations.ListData())
                {
                    contains |= contextScannedLocation.Equals(scannedLocation);
                }
                return contains;
            });

            return await task;
        }
    }
}