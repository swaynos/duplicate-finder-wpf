using DuplicateFinder.Framework;
using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using DuplicateFinder.Views;
using FileHashRepository;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace DuplicateFinder.ViewModels
{
    public class HomePageViewModel : BindableBase
    {
        private IFolderBrowserDialogWrapper _folderBrowserDialog;
        private IScannedFileStore _scannedFileStore;
        private DelegateCommand _add;
        private DelegateCommand _remove;
        private DelegateCommand _scan;
        private string _userAppDataPath;

        public ObservableCollection<ScanLocation> Locations { get; set; }

        public ScanLocation SelectedLocation { get; set; } 

        /// <summary>
        /// Internally exposed constructor for Unit Testing
        /// </summary>
        internal HomePageViewModel(
            IFolderBrowserDialogWrapper folderBrowserDialog, 
            IScannedFileStore scannedFileStore, 
            string userAppDataPath)
        {
            _folderBrowserDialog = folderBrowserDialog;
            _scannedFileStore = scannedFileStore;
            _userAppDataPath = userAppDataPath;
            this.Locations = new ObservableCollection<ScanLocation>();
            _add =  DelegateCommand.Create(AddLocation);
            _remove = DelegateCommand.Create(RemoveLocation, false);
            _scan = DelegateCommand.Create(ShowScanPage, false);
            this.Add = _add;
            this.Remove = _remove;
            this.Scan = _scan;
            this.PageLoaded = AsyncCommand.Create(OnPageLoaded);
        }

        public HomePageViewModel() : this(
            new FolderBrowserDialogWrapper(), 
            new ScannedFileStore(), 
            Application.UserAppDataPath)
        {
        }

        public IAsyncCommand PageLoaded { get; set; }

        public ICommand Add { get; private set; }

        public ICommand Remove { get; private set; }

        public ICommand Scan { get; private set; }

        /// <summary>
        /// Show a folder browser dialog to allow the user to select a location, and add it to Locations. Any exceptions would be fatal.
        /// </summary>
        private void AddLocation()
        {
            using (CommonDialog dialog = _folderBrowserDialog.GetNewFolderBrowserDialog())
            {
                DialogResult result = _folderBrowserDialog.ShowDialogWrapper(dialog);
                if (result == DialogResult.OK)
                {
                    string resultString = _folderBrowserDialog.GetSelectedPathFromDialog(dialog);
                    ScanLocation resultScanLocation = new ScanLocation(resultString);
                    if (!Locations.Contains(resultScanLocation))
                    {
                        Locations.Add(resultScanLocation);
                    }
                }
            }
            ToggleButtons();
        }

        /// <summary>
        /// Remove the selected location from Locations
        /// </summary>
        private void RemoveLocation()
        {
            if (SelectedLocation != null)
            {
                Locations.Remove(SelectedLocation);
            }
            ToggleButtons();
        }

        /// <summary>
        /// Transitions to the ScanPage
        /// </summary>
        private void ShowScanPage()
        {
            ScanPage scanPage = new ScanPage();
            ScanPageViewModel scanPageViewModel = scanPage.DataContext as ScanPageViewModel;
            scanPageViewModel.SetScannedFileStore(_scannedFileStore);
            scanPageViewModel.Locations = new List<ScanLocation>(Locations);
            App.NavigationService.Navigate(scanPage);
        }

        /// <summary>
        /// Handles the logic to enable and disable buttons based on the state of the Model
        /// </summary>
        private void ToggleButtons()
        {
            if (Locations.Count > 0)
            {
                _remove.IsEnabled = true;
                _scan.IsEnabled = true;
            }
            else
            {
                _remove.IsEnabled = false;
                _scan.IsEnabled = false;
            }
        }

        /// <summary>
        /// Handles when the HomePage is loaded
        /// </summary>
        private async Task OnPageLoaded()
        {
            string dataFilePath = Path.Combine(_userAppDataPath, "data.json");
            await _scannedFileStore.LoadScannedFileStoreFromFileAsync(dataFilePath);
        }
    }
}
