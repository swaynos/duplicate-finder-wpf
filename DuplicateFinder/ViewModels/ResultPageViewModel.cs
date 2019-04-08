using DuplicateFinder.Framework;
using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using FileHashRepository;
using FileHashRepository.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace DuplicateFinder.ViewModels
{
    public enum BackgroundColor
    {
        Transparent=0x0,
        DarkGray=0x1
    }

    public class ResultPageViewModel : BaseViewModel
    {
        private string _searchFilter;
        private ILogger _logger;
        private IProcess _process;
        private AsyncCommand<object> _preview;
        private AsyncCommand<object> _recycle;
        private IRecycleFile _recycleFile;

        public ObservableCollection<ScanResult> Duplicates { get; set; }

        public CollectionViewSource DuplicatesViewSource { get; set; }

        public IAsyncCommand PageLoaded { get; set; }

        public IAsyncCommand Preview { get; set; }

        public IAsyncCommand Recycle { get; set; }

        public ICommand SelectedItemsChangedCommand { get; set; }

        public string SearchFilter
        {
            get { return _searchFilter; }
            set
            {
                if (value != null)
                {
                    _searchFilter = value;
                    AddFilter();
                }
                DuplicatesViewSource.View.Refresh(); // important to refresh your View
            }
        }

        private void AddFilter()
        {
            DuplicatesViewSource.Filter -= new FilterEventHandler(Filter);
            DuplicatesViewSource.Filter += new FilterEventHandler(Filter);
        }

        private void Filter(object sender, FilterEventArgs e)
        {
            var src = e.Item as ScanResult;
            if (src == null)
            {
                e.Accepted = false;
            }
            else if (src.FilePath != null && !src.FilePath.Contains(SearchFilter))
            {
                e.Accepted = false;
            }
        }

        public ResultPageViewModel() : this(LogManager.GetCurrentClassLogger(), new Process(), new RecycleFile(), Application.UserAppDataPath, null)
        {
        }

        internal ResultPageViewModel(
            ILogger logger, 
            IProcess process, 
            IRecycleFile recycleFile, 
            string userAppDataPath,
            IScannedFileStore scannedFileStore) : base(scannedFileStore, userAppDataPath)
        {
            _logger = logger;
            _process = process;
            _recycle = AsyncCommand.Create(() => RecycleSelectionAsync());
            _preview = AsyncCommand.Create(() => PreviewSelectionAsync(), false);
            _recycleFile = recycleFile;
            _userAppDataPath = userAppDataPath;

            Duplicates = new ObservableCollection<ScanResult>();
            DuplicatesViewSource = new CollectionViewSource();
            DuplicatesViewSource.Source = Duplicates;
            PageLoaded = AsyncCommand.Create(() => PageLoadedAsync());
            Recycle = _recycle;
            Preview = _preview;
            SelectedItemsChangedCommand = DelegateCommand.Create(ToggleButtons);
        }

        /// <summary>
        /// Takes the scanned files and adds them to the <see cref="Duplicates"/> collection. 
        /// The ScannedFiles will be sorted by Hash before being added to the ViewModel.
        /// Internally exposed for unit testing.
        /// It is assumed that <paramref name="scannedFiles"/> is sorted by Hash before calling.
        /// </summary>
        /// <param name="scannedFiles">The scanned files to add</param>
        internal void AddScannedFiles(List<ScannedFile> scannedFiles)
        {
            byte[] previousHash = null;
            BackgroundColor color = BackgroundColor.Transparent;
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();

            foreach (ScannedFile scannedFile in scannedFiles)
            {
                // If the hash is not same as the previous hash flip the same color
                if (previousHash != null && !comparer.Equals(previousHash, scannedFile.Hash))
                {
                    // If there are ever more than two BackgroundColor types, this flipping logic
                    // will need to be revisited.
                    color = 1 - color;
                }
                ScanResult scanResult = new ScanResult()
                {
                    FilePath = scannedFile.Path,
                    Hash = scannedFile.Hash,
                    Background = color.ToString(),
                    IsSelected = false
                };
                Duplicates.Add(scanResult);
                previousHash = scannedFile.Hash;
            }
        }

        /// <summary>
        /// Call <see cref="IProcess.StartAsync(string)"/> for each selected duplicate file.
        /// </summary>
        private async Task PreviewSelectionAsync()
        {
            var selectedDuplicates = Duplicates.Where(t => t.IsSelected).Select(t => t.FilePath);
            foreach (string file in selectedDuplicates)
            {
                await _process.StartAsync(file);
            }
        }

        /// <summary>
        /// Send the selected files to the recycle bin
        /// </summary>
        private async Task RecycleSelectionAsync()
        {
            bool suppressRecycleFileDialog = Properties.Settings.Default.SuppressRecycleFileDialog;
            List<string> recycledItems = new List<string>();

            var selectedItems = Duplicates.Where(t => t.IsSelected);
            foreach (ScanResult itemToRecycle in selectedItems)
            {
                if (await _recycleFile.RecycleAsync(itemToRecycle.FilePath, suppressRecycleFileDialog))
                {
                    var result = Duplicates.Remove(itemToRecycle);
                }
            }
        }

        /// <summary>
        /// Logic run when the page is loaded.
        /// </summary>
        private async Task PageLoadedAsync()
        {
            if (_scannedFileStore != null)
            {
                // Save the scan results
                await _scannedFileStore.SaveScannedFileStoreToFileAsync(GetDataFilePath());

                // Load the view model with the scanned file data
                // AddScannedFiles() must be run on the UI Thread
                AddScannedFiles(await _scannedFileStore.ListDuplicateFilesAsync());
            }

            ToggleButtons();
        }

        /// <summary>
        /// Handles the logic to enable and disable buttons based on the state of the Model
        /// </summary>
        private void ToggleButtons()
        {
            if (Duplicates.Count > 0)
            {
                _preview.IsEnabled = true;
                _recycle.IsEnabled = true;
            }
        }
    }
}