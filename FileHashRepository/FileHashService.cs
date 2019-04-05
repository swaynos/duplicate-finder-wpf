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
        /// Provides the list of ScannedFile. Path's filtered by the locationPaths.
        /// </summary>
        /// <param name="locationPaths">Optional: The location to return scanned files from.
        /// <para>If null everything will be returned.</para>
        /// <para>If empty, an empty List will be returned.</para>
        /// </param>
        /// <returns>A List of strings which contain the ScannedFile.Path's</returns>
        public async Task<List<string>> ListScannedFilePathsAsync(List<string> locationPaths)
        {
            IQueryable<ScannedFile> scannedFiles = await FindScannedFilesByLocationPathsAsync(locationPaths);
            return scannedFiles.Select(t => t.Path).ToList();
            // ToDo: Remove
            //List<string> scannedFilePaths;

            //if (locationPaths == null)
            //{
            //    scannedFilePaths =  _scannedFiles.ListData().Select(t => t.Path).ToList();
            //}
            //else if (locationPaths.Count == 0)
            //{
            //    scannedFilePaths = new List<string>();
            //}
            //else
            //{
            //    scannedFilePaths = new List<string>();
            //    // ToDo: Optimize. Can we at least make it an async operation?
            //    foreach (ScannedFile scannedFile in _scannedFiles.ListData().AsEnumerable())
            //    {
            //        // C:\Foo\Bar
            //        // C:\Foo\BarFoo
            //        // Both will be returned, but we only want #1
            //        foreach(string locationPath in locationPaths)
            //        {
            //            if (scannedFile.Path.Equals(locationPath, StringComparison.InvariantCultureIgnoreCase)
            //                || (scannedFile.Path.StartsWith(locationPath, StringComparison.InvariantCultureIgnoreCase)
            //                && scannedFile.Path.Length > locationPath.Length 
            //                && scannedFile.Path[locationPath.Length] == '\\'))
            //            {
            //                scannedFilePaths.Add(scannedFile.Path);
            //            }
            //        }
            //    }
            //}

            //return scannedFilePaths;
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
        /// Removes all ScannedFiles and ScannedLocations from the storage Backend at the given location
        /// </summary>
        /// <param name="locationPaths">Optional: The location to purge scanned files from.
        /// <para>If null everything will be purged.</para> // ToDo: Unit Test
        /// <para>If empty, nothing will be purged.</para></param> // ToDo: Unit Test
        public async Task PurgeScannedLocationsAsync(List<string> locationPaths)
        {
            if (locationPaths == null)
            {
                _scannedFiles.PurgeData(_scannedFiles.ListData());
                _scannedLocations.PurgeData(_scannedLocations.ListData());
            }
            else if (locationPaths.Count > 0)
            {
                _scannedFiles.PurgeData(await FindScannedFilesByLocationPathsAsync(locationPaths));
                _scannedLocations.PurgeData(await FindScannedLocationsByLocationPathAsync(locationPaths));
            }
        }

        /// <summary>
        /// Removes all ScannedFile records from the storage backend by a file path
        /// </summary>
        /// <param name="filePath">The file path of the file record(s), will compare the whole path of the ScannedFile</param>
        /// <returns>The number of ScannedFiles removed</returns> // ToDo: This is unnecessary
        public async Task<int> RemoveScannedFilesByFilePathAsync(string filePath)
        {
            // ToDo: Not an async method
            IQueryable<ScannedFile> scannedFiles = _scannedFiles.ListData().Where(t => t.Path.Equals(filePath));
            int removedRecords =  scannedFiles.Count(); 
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
            return _scannedFiles.ListData()
                .GroupBy(t => t.Hash, new ScannedFileHashComparer())
                .Where(t => t.Count() > 1)
                .SelectMany(group => group).ToList();
        }

        // ToDo: Unit Test that the entire collection is returned
        /// <summary>
        /// Lists the entire collection of scanned locations
        /// </summary>
        public IEnumerable<ScannedLocation> ListScannedLocations()
        {
            return _scannedLocations.ListData();
        }

        // ToDo: Unit Test that the entire collection is returned
        /// <summary>
        /// Lists the entire collection of scanned files
        /// </summary>
        public IEnumerable<ScannedFile> ListScannedFiles()
        {
            return _scannedFiles.ListData();
        }

        /// <summary>
        /// Update the <see cref="IDataCache{T}"/> with the provided files and locations.
        /// </summary>
        /// <param name="scannedFiles"></param>
        /// <param name="scannedLocations"></param>
        public void UpdateDataCaches(IDataCache<ScannedFile> scannedFiles, IDataCache<ScannedLocation> scannedLocations)
        {
            _scannedFiles = scannedFiles;
            _scannedLocations = scannedLocations;
        }

        /// <summary>
        /// Iterate through the ScannedFiles to determine if there is an equal ScannedFile
        /// </summary>
        /// <param name="scannedFile">The ScannedFile to compare against the ScannedFiles in the data cache</param>
        /// <returns>true/false if the ScannedFile is contained in the data cache</returns>
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
        /// Helper method used in <see cref="ListScannedFilePathsAsync(List{string})"/> and <see cref="PurgeScannedLocationsAsync(List{string})"/>
        /// </summary>
        /// <param name="locationPaths">Optional: The location to return scanned files from.
        /// <para>If null everything will be returned.</para>
        /// <para>If empty, an empty List will be returned.</para>
        /// </param>
        /// <returns>A collection of ScannedFile's</returns>
        private async Task<IQueryable<ScannedFile>> FindScannedFilesByLocationPathsAsync(List<string> locationPaths)
        {
            List<ScannedFile> scannedFilesList;

            if (locationPaths == null)
            {
                return _scannedFiles.ListData();
            }
            else if (locationPaths.Count == 0)
            {
                scannedFilesList = new List<ScannedFile>();
            }
            else
            {
                scannedFilesList = new List<ScannedFile>();
                // ToDo: Optimize. Can we at least make it an async operation?
                foreach (ScannedFile scannedFile in _scannedFiles.ListData().AsEnumerable())
                {
                    // C:\Foo\Bar\File.txt
                    // C:\Foo\BarFoo\File.txt
                    // We only want #1 with C:\Foo\Bar input
                    foreach (string locationPath in locationPaths)
                    {
                        if (scannedFile.Path.Equals(locationPath, StringComparison.InvariantCultureIgnoreCase)
                            || (scannedFile.Path.StartsWith(locationPath, StringComparison.InvariantCultureIgnoreCase)
                            && scannedFile.Path.Length > locationPath.Length
                            && scannedFile.Path[locationPath.Length] == '\\'))
                        {
                            scannedFilesList.Add(scannedFile);
                        }
                    }
                }
            }
            return scannedFilesList.AsQueryable();
        }

        /// <summary>
        /// Helper method used in <see cref="PurgeScannedLocationsAsync(List{string})"/>
        /// </summary>
        /// <param name="locationPaths">Optional: The location to return scanned locations from.
        /// <para>If null everything will be returned.</para>
        /// <para>If empty, an empty List will be returned.</para>
        /// </param>
        /// <returns>A collection of ScannedLocation's</returns>
        private async Task<IQueryable<ScannedLocation>> FindScannedLocationsByLocationPathAsync(List<string> locationPaths)
        {
            List<ScannedLocation> scannedLocationsList;

            if (locationPaths == null)
            {
                return _scannedLocations.ListData();
            }
            else if (locationPaths.Count == 0)
            {
                scannedLocationsList = new List<ScannedLocation>();
            }
            else
            {
                scannedLocationsList = new List<ScannedLocation>();
                // ToDo: Optimize. Can we at least make it an async operation?
                foreach (ScannedLocation scannedLocation in _scannedLocations.ListData().AsEnumerable())
                {
                    // C:\Foo\Bar
                    // C:\Foo\BarFoo
                    // We only want #1 with C:\Foo\Bar input
                    foreach (string locationPath in locationPaths)
                    {
                        if (scannedLocation.Path.Equals(locationPath, StringComparison.InvariantCultureIgnoreCase)
                            || (scannedLocation.Path.StartsWith(locationPath, StringComparison.InvariantCultureIgnoreCase)
                            && scannedLocation.Path.Length > locationPath.Length
                            && scannedLocation.Path[locationPath.Length] == '\\'))
                        {
                            scannedLocationsList.Add(scannedLocation);
                        }
                    }
                }
            }
            return scannedLocationsList.AsQueryable();
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