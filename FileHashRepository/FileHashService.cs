using FileHashRepository.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public class FileHashService : IFileHashService
    {
        private FileHashEntities _context;

        public FileHashService(FileHashEntities context)
        {
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        /// <summary>
        /// Inserts a ScannedFile into the storage backend
        /// </summary>
        /// <param name="scannedFile">The ScannedFile to insert</param>
        public async Task InsertScannedFileAsync(ScannedFile scannedFile)
        {
            if(!await ScannedFilesContains(scannedFile))
            {
                _context.ScannedFiles.Add(scannedFile);
                await _context.SaveChangesAsync();
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
                _context.ScannedLocations.Add(scannedLocation);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Provides the list of ScannedFile.Path's filtered by the locationPaths
        /// </summary>
        /// <param name="locationPaths">Optional: The location to return scanned files from.
        /// <para>If null everything will be returned.</para>
        /// <para>If empty an empty List will be returned.</para>
        /// </param>
        /// <returns>A List of strings which contain the ScannedFile.Path's</returns>
        public async Task<List<string>> ListScannedFilePathsAsync(List<string> locationPaths)
        {
            List<string> scannedFilePaths;
            const string query = @"SELECT A.[Path] FROM 
                    (SELECT
                    Left([Path], LEN([Path]) - CHARINDEX('\', REVERSE([Path]))) As [Directory],
                    [Path] as [Path]
                    FROM [dbo].[ScannedFiles]) A
                WHERE A.[Directory] = '{0}' OR A.[Directory] Like '{0}\%'";

            if (locationPaths == null)
            {
                scannedFilePaths = await _context.ScannedFiles.Select(t => t.Path).ToListAsync();
            }
            else
            {
                scannedFilePaths = new List<string>();
                foreach (string locationPath in locationPaths)
                {
                    scannedFilePaths.AddRange(_context.Database.SqlQuery<string>(SqlQuery.FormatSqlQuery(query, locationPath)).AsEnumerable());
                }
            }

            return scannedFilePaths;
        }

        /// <summary>
        /// Provides the list of all ScannedLocation.Path's
        /// </summary>
        /// <returns>A List of strings which contain the ScannedLocation.Path's</returns>
        public async Task<List<string>> ListScannedLocationsAsync()
        {
            return await _context.ScannedLocations.Select(t => t.Path).ToListAsync();
        }

        /// <summary>
        /// Removes all ScannedFiles from the storage Backend at the given location
        /// </summary>
        /// <param name="locationPaths">Optional: The location to purge scanned files from.
        /// <para>If null everything will be purged.</para>
        /// <para>If empty, nothing will be purged.</para></param>
        public async Task PurgeScannedLocationsAsync(List<string> locationPaths)
        {
            IQueryable<ScannedLocation> scannedLocations;
            const string command = @"DELETE FROM [dbo].[ScannedFiles] 
                WHERE Left([Path], LEN([Path]) - CHARINDEX('\', REVERSE([Path]))) = '{0}'
                OR  Left([Path], LEN([Path]) - CHARINDEX('\', REVERSE([Path]))) Like '{0}\%'";

            if (locationPaths == null)
            {
                scannedLocations = _context.ScannedLocations;
                _context.ScannedFiles.RemoveRange(_context.ScannedFiles);
            }
            else
            {
                scannedLocations = _context.ScannedLocations.Where(t => locationPaths.Contains(t.Path));
                foreach(string locationPath in locationPaths)
                {
                    _context.Database.ExecuteSqlCommand(SqlQuery.FormatSqlQuery(command, locationPath));
                }
            }

            _context.ScannedLocations.RemoveRange(scannedLocations);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes all ScannedFile records from the storage backend by a file path
        /// </summary>
        /// <param name="filePath">The file path of the file record(s)</param>
        /// <returns>The number of ScannedFiles removed</returns>
        public async Task<int> RemoveScannedFilesByFilePathAsync(string filePath)
        {
            IQueryable<ScannedFile> scannedFiles = _context.ScannedFiles.Where(t => t.Path.Equals(filePath));
            _context.ScannedFiles.RemoveRange(scannedFiles);
            int savedChanges = await _context.SaveChangesAsync();
            return savedChanges;
        }

        /// <summary>
        /// Returns all ScannedFile entities who have a Hash that is present in the storage backend
        /// more than once.
        /// </summary>
        /// <returns>A list of ScannedFile entities</returns>
        public async Task<List<ScannedFile>> ReturnDuplicatesAsync()
        {
            IQueryable<ScannedFile> scannedFiles = 
                _context.ScannedFiles
                    .GroupBy(t => t.Hash)
                    .Where(t => t.Count() > 1)
                    .SelectMany(group => group);
            return await scannedFiles.ToListAsync();
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
                foreach (ScannedFile contextScannedFile in _context.ScannedFiles)
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
                foreach (ScannedLocation contextScannedLocation in _context.ScannedLocations)
                {
                    contains |= contextScannedLocation.Equals(scannedLocation);
                }
                return contains;
            });

            return await task;
        }
    }
}