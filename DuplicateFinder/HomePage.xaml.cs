using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace DuplicateFinder
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private IFolderBrowserDialogWrapper _folderBrowserDialog;

        public HomePageModel Model { get; set; }

        internal HomePage(IFolderBrowserDialogWrapper folderBrowserDialog)
        {
            _folderBrowserDialog = folderBrowserDialog;
            Model = new HomePageModel();
            InitializeComponent();
        }

        public HomePage() : this(new FolderBrowserDialogWrapper())
        {
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void AddButton_Click(object sender, RoutedEventArgs e)
        {
            using (CommonDialog dialog = _folderBrowserDialog.GetNewFolderBrowserDialog())
            {
                DialogResult result = _folderBrowserDialog.ShowDialogWrapper(dialog);
                if (result == DialogResult.OK)
                {
                    string resultString = _folderBrowserDialog.GetSelectedPathFromDialog(dialog);
                    if (!Model.Locations.Contains(resultString))
                    {
                        Model.Locations.Add(resultString);
                        LocationsListBox.Items.Add(resultString);
                    }
                }
            }
            ToggleButtons();
        }

        public void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LocationsListBox.SelectedItem != null)
            {
                Model.Locations.RemoveAt(
                     Model.Locations.IndexOf(LocationsListBox.SelectedItem.ToString()));
                LocationsListBox.Items.RemoveAt(
                    LocationsListBox.Items.IndexOf(LocationsListBox.SelectedItem));
            }
            ToggleButtons();
        }

        public async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            ScanningController scanningController = new ScanningController(Model.Locations);
            ScanPage scanPage = new ScanPage(scanningController);

            this.NavigationService.Navigate(scanPage);
            await scanPage.StartScanAsync();

        }

        /// <summary>
        /// Handles the logic to enable and disable buttons based on the state of the Model
        /// </summary>
        private void ToggleButtons()
        {
            if (Model.Locations.Count > 0)
            {
                RemoveButton.IsEnabled = true;
                ScanButton.IsEnabled = true;
            }
            else
            {
                RemoveButton.IsEnabled = false;
                ScanButton.IsEnabled = false;
            }
        }
    }
}
