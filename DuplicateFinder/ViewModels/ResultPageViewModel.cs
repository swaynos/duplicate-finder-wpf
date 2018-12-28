using DuplicateFinder.Framework;
using DuplicateFinder.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System;
using NLog;
using System.Threading.Tasks;
using DuplicateFinder.Utilities;
using FileHashRepository;

namespace DuplicateFinder.ViewModels
{
    public class ResultPageViewModel : BindableBase
    {
        private string _searchFilter;
        private ILogger _logger;
        private IProcess _process;
        private AsyncCommand<object> _preview;
        private AsyncCommand<object> _recycle;
        private IRecycleFile _recycleFile;

        public ObservableCollection<ScanResult> Duplicates { get; set; }

        public CollectionViewSource DuplicatesViewSource { get; set; }

        public ICommand Loaded { get; set; }

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

        public ResultPageViewModel() : this(LogManager.GetCurrentClassLogger(), new Process(), new RecycleFile())
        {
        }

        internal ResultPageViewModel(ILogger logger, IProcess process, IRecycleFile recycleFile)
        {
            _logger = logger;
            _process = process;
            _recycle = AsyncCommand.Create(() => RecycleSelectionAsync());
            _preview = AsyncCommand.Create(() => PreviewSelection(), false);
            _recycleFile = recycleFile;

            Duplicates = new ObservableCollection<ScanResult>();
            DuplicatesViewSource = new CollectionViewSource();
            DuplicatesViewSource.Source = Duplicates;
            Loaded = DelegateCommand.Create(ToggleButtons);
            Recycle = _recycle;
            Preview = _preview;
            SelectedItemsChangedCommand = DelegateCommand.Create(ToggleButtons);
        }

        private async Task PreviewSelection()
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

        /// <summary>
        /// Takes the scanned files and adds them to the <see cref="Duplicates"/> collection. 
        /// The ScannedFiles will be sorted by Hash before being added to the ViewModel.
        /// </summary>
        /// <param name="scannedFiles">The scanned files to add</param>
        /// <exception cref="NullReferenceException">Will return a NullReferenceException if <paramref name="scannedFiles"/> is null.</exception>
        internal void AddScannedFiles(List<ScannedFile> scannedFiles)
        {
            if (scannedFiles == null)
                throw new NullReferenceException();

            byte[] previousHash = null;
            // ToDo: Unit Test
            // ToDo: Order By? Verify that this is being ordered earlier in the stack, and move here.
            foreach (ScannedFile scannedFile in scannedFiles)
            {
                ScanResult scanResult = new ScanResult()
                {
                    FilePath = scannedFile.Path,
                    Hash = scannedFile.Hash,
                    IsSelected = false
                };
                // ToDo: Implement Colors
                // If the hash is the same as the previous hash use the same color
                // Else flip the color
                Duplicates.Add(scanResult);
                previousHash = scannedFile.Hash;
            }
        }
    }
}