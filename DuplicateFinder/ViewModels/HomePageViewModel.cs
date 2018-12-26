using DuplicateFinder.Framework;
using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using DuplicateFinder.Views;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace DuplicateFinder.ViewModels
{
    public class HomePageViewModel : BindableBase
    {
        private IFolderBrowserDialogWrapper _folderBrowserDialog;
        private DelegateCommand _add;
        private DelegateCommand _remove;
        private DelegateCommand _scan;

        public ObservableCollection<ScanLocation> Locations { get; set; }

        public ScanLocation SelectedLocation { get; set; } 

        internal HomePageViewModel(IFolderBrowserDialogWrapper folderBrowserDialog)
        {
            _folderBrowserDialog = folderBrowserDialog;
            this.Locations = new ObservableCollection<ScanLocation>();
            _add =  DelegateCommand.Create(AddLocation);
            _remove = DelegateCommand.Create(RemoveLocation, false);
            _scan = DelegateCommand.Create(ShowScanPage, false);
            this.Add = _add;
            this.Remove = _remove;
            this.Scan = _scan;
        }
        public HomePageViewModel() : this(new FolderBrowserDialogWrapper())
        {
        }

        public ICommand Add { get; private set; }
        public ICommand Remove { get; private set; }
        public ICommand Scan { get; private set; }

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

        private void RemoveLocation()
        {
            if (SelectedLocation != null)
            {
                Locations.Remove(SelectedLocation);
            }
            ToggleButtons();
        }

        private void ShowScanPage()
        {
            ScanPage scanPage = new ScanPage();
            ScanPageViewModel scanPageViewModel = scanPage.DataContext as ScanPageViewModel;
            scanPageViewModel.Locations = Locations;
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
    }
}
