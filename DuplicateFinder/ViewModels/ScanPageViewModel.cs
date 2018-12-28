using DuplicateFinder.Framework;
using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using DuplicateFinder.Views;
using FileHashRepository;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinder.ViewModels
{
    public class ScanPageViewModel : BindableBase
    {
        private ILogger _logger;
        private int _progress;
        private List<ScannedFile> _returnDuplicatesResult;
        private IScannedFileStore _scannedFileStore;

        public List<ScanLocation> Locations { get; set; }

        /// <summary>
        /// Displays the progress of the scanning
        /// </summary>
        public int Progress {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public IAsyncCommand Loaded { get; private set; }

        /// <summary>
        /// Event to bind to for when the scanning is completed.
        /// </summary>
        public event EventHandler ScanComplete;

        /// <summary>
        /// Called when the scanning is completed, raises ScanComplete 
        /// </summary>
        protected virtual void OnScanComplete()
        {
            ScanComplete?.Invoke(this, EventArgs.Empty);
        }

        public ScanPageViewModel() : this(LogManager.GetCurrentClassLogger(), new ScannedFileStore())
        {
        }

        public ScanPageViewModel(ILogger logger, IScannedFileStore scannedFileStore)
        {
            _logger = logger;
            _scannedFileStore = scannedFileStore;
            Locations = new List<ScanLocation>();
            Progress = 0;
            ScanComplete += ShowResultPage;
            Loaded = AsyncCommand.Create(OnLoaded);
        }

        /// <summary>
        /// Task that should fire when the page reports that it is loaded
        /// </summary>
        public async Task OnLoaded()
        {
            await BeginScanAsync();
            _returnDuplicatesResult = await RetrieveDuplicatesAsync();
            OnScanComplete();
        }

        /// <summary>
        /// <para>Performs the scan process</para>
        /// <para>First the storage backend is queried for existing scanned locations.
        /// Then these existing locations are compared with the user selected locations to 
        /// determine which locations are "new" and which are "rescans".</para> 
        /// <para>Locations (and records) that are existing but not now user selected 
        /// will be purged via
        /// <see cref="ScannedFileStore.PurgeLocationsAsync(List{string})"/></para>
        /// <para>After determining these locations 
        /// <see cref="ScannedFileStore.ScanLocationsAsync(List{string}, IProgress{int})"/> is called for
        /// new scans, and <see cref="ScannedFileStore.RescanLocationsAsync(List{string}, IProgress{int})"/> 
        /// for rescans.</para>
        /// <para>Any <see cref="Exception"/> should be handled and logged as an error.</para>
        /// </summary>
        internal async Task BeginScanAsync()
        {
            List<string> locations = Locations.Select(t => t.Path).ToList();

            ScanningProgress progress = new ScanningProgress((d) =>
            {
                Progress = (int)d;
            }, 0, 2);

            try
            {
                // Previously scanned locations
                List<string> scannedLocations = await _scannedFileStore.ListScannedLocationsAsync();

                // Determine all locations that are not in scannedLocations, these are new locations
                List<string> newLocations = locations.Except(scannedLocations).ToList();

                // Determine all scannedLocations that are in Locations, these are rescan locations
                List<string> rescanLocations = scannedLocations.Where(t => locations.Contains(t)).ToList();

                // Determine all locations that are in scannedLocations, but not in Locations.
                // We will purge scanned files from these locations
                List<string> purgeLocations = scannedLocations.Except(locations).ToList();

                // Purge any unneeded locations
                await _scannedFileStore.PurgeLocationsAsync(purgeLocations);

                // Scan new locations
                await _scannedFileStore.ScanLocationsAsync(newLocations, progress);

                progress.Addend = 50; // Update the Addend so that value will be reporeted correctly

                // Rescan previously scanned locations
                await _scannedFileStore.RescanLocationsAsync(rescanLocations, progress);

                progress.Report(100);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred during the scanning process.");
            }
        }

        /// <summary>
        /// Retrieve the duplicate files from the storage backend 
        /// <see cref="ScannedFileStore.ListDuplicateFilesAsync"/>.
        /// Any exception should be handled and logged as an error. 
        /// In the event of an exception an empty list will be returned.
        /// </summary>
        /// <returns>The collection of ScannedFile duplicates </returns>
        internal async Task<List<ScannedFile>> RetrieveDuplicatesAsync()
        {
            try
            {
                return await _scannedFileStore.ListDuplicateFilesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while retrieving duplicates from the storage backend.");
            }
            return new List<ScannedFile>();
        }

        /// <summary>
        /// Transition to the ResultPage
        /// </summary>
        private void ShowResultPage(object sender, EventArgs e)
        {
            ResultPage resultPage = new ResultPage();
            ResultPageViewModel resultPageViewModel = resultPage.DataContext as ResultPageViewModel;
            resultPageViewModel.AddScannedFiles(_returnDuplicatesResult);
            App.NavigationService.Navigate(resultPage);
        }
    }
}
