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

namespace DuplicateFinder.ViewModels
{
    public class ResultPageViewModel : BindableBase
    {
        private string _searchFilter;
        private ILogger _logger;
        private DelegateCommand _preview;
        private AsyncCommand<object> _recycle;
        private IRecycleFile _recycleFile;

        public ObservableCollection<ScanResult> Duplicates { get; set; }

        public CollectionViewSource DuplicatesViewSource { get; set; }

        public ICommand Loaded { get; set; }

        public ICommand Preview { get; set; } // ToDo: Shouldn't this be an async command?

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

        public ResultPageViewModel() : this(LogManager.GetCurrentClassLogger(), new RecycleFile())
        {
        }

        internal ResultPageViewModel(ILogger logger, IRecycleFile recycleFile)
        {
            _logger = logger;
            _recycle = AsyncCommand.Create(() => RecycleSelection());
            _preview = DelegateCommand.Create(PreviewSelection, false);
            _recycleFile = recycleFile;

            Duplicates = new ObservableCollection<ScanResult>();
            DuplicatesViewSource = new CollectionViewSource();
            DuplicatesViewSource.Source = Duplicates;
            Loaded = DelegateCommand.Create(ToggleButtons);
            Recycle = _recycle;
            Preview = _preview;
            SelectedItemsChangedCommand = DelegateCommand.Create(ToggleButtons);
        }

        private void PreviewSelection()
        {
            var selectedDuplicates = Duplicates.Where(t => t.IsSelected).Select(t => t.FilePath);
            foreach (string file in selectedDuplicates)
            {
                // ToDo: Move into it's own class
                try
                {
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = file;
                    process.Start();
                }
                catch (Win32Exception ex)
                {
                    _logger.Log(LogLevel.Error, ex);
                }
            }
        }

        private async Task RecycleSelection()
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