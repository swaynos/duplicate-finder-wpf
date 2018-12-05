using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using FileHashRepository;
using FileHashRepository.Utilities;
using NLog;

namespace DuplicateFinder
{
    internal class ScanningController : IScanningController
    {
        private IScannedFileStore _scannedFileStore;

        private ILogger _logger;

        public List<string> Locations { get; set; }

        internal ScanningController(List<string> locations, IScannedFileStore scannedFileStore, ILogger logger)
        {
            _scannedFileStore = scannedFileStore;
            _logger = logger;
            Locations = locations;
        }

        public ScanningController(List<string> locations) : this(locations, new ScannedFileStore(), LogManager.GetCurrentClassLogger())
        {
        }

        public async Task BeginScanAsync(RangeBase control)
        {
            ScanningProgress progress = new ScanningProgress(control, 0, 3);

            try
            {
                // Previously scanned locations
                List<string> scannedLocations = await _scannedFileStore.ListScannedLocationsAsync();

                // Determine all locations that are not in scannedLocations, these are new locations
                List<string> newLocations = Locations.Except(scannedLocations).ToList();

                // Determine all scannedLocations that are in Locations, these are rescan locations
                List<string> rescanLocations = scannedLocations.Where(t => Locations.Contains(t)).ToList();

                // Determine all locations that are in scannedLocations, but not in Locations.
                // We will purge scanned files from these locations
                List<string> purgeLocations = scannedLocations.Except(Locations).ToList();

                // Purge any unneeded locations
                await _scannedFileStore.PurgeLocationsAsync(purgeLocations);

                // Scan new locations
                await _scannedFileStore.ScanLocationsAsync(newLocations, progress);

                progress.Addend = 50; // Update the Addend so that value will be reporeted correctly

                // Rescan previously scanned locations
                await _scannedFileStore.RescanLocationsAsync(rescanLocations, progress);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred during the scanning process.");
            }
        }

        public async Task<List<DuplicateResultModel>> RetrieveDuplicatesAsync()
        {
            List<DuplicateResultModel> results = new List<DuplicateResultModel>();
            try
            {
                List<ScannedFile> duplicateFiles = await _scannedFileStore.ListDuplicateFilesAsync();

                foreach (var duplicateFile in duplicateFiles.GroupBy(t => t.Hash, new ScannedFileHashComparer()))
                {
                    DuplicateResultModel duplicateResultModel = new DuplicateResultModel();
                    duplicateResultModel.DuplicateFiles = duplicateFile.Select(t => {
                            return new FileModel()
                            {
                                FileName = t.Name,
                                LocationPath = t.Path
                            };
                    }).ToList();
                    
                    results.Add(duplicateResultModel);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while retrieving duplicates from the storage backend.");
            }

            return results;
        }
    }
}
