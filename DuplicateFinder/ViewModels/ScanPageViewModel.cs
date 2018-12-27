using DuplicateFinder.Framework;
using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using DuplicateFinder.Views;
using FileHashRepository;
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
        private List<ScannedFile> returnDuplicatesResult { get; set; }

        public ObservableCollection<ScanLocation> Locations { get; set; }

        public int Progress { get; set; }

        public IAsyncCommand Loaded { get; private set; }

        //ToDo: Unit Test this Event
        public event EventHandler ScanComplete;

        protected void OnScanComplete()
        {
            ScanComplete?.Invoke(this, EventArgs.Empty);
        }

        public ScanPageViewModel()
        {
            Locations = new ObservableCollection<ScanLocation>();
            Progress = 0;
            ScanComplete += (sender, e) =>
            {
                ShowResultPage();
            };
            Loaded = AsyncCommand.Create(() => BeginScanAsync());
        }

        public async Task BeginScanAsync()
        {
            // ToDo: Implement the async Scan
            returnDuplicatesResult = new List<ScannedFile>()
            {
                new ScannedFile()
                {
                    Hash = new byte[32],
                    Path = "C:\\foo\\foo.txt",
                    Name = "foo.txt"
                },
                new ScannedFile()
                {
                    Hash = new byte[32],
                    Path = "C:\\bar\\Bar.txt",
                    Name = "Bar.txt"
                }
            };
            Progress = 100;
            OnScanComplete();
        }

        private void ShowResultPage()
        {
            ResultPage resultPage = new ResultPage();
            ResultPageViewModel resultPageViewModel = resultPage.DataContext as ResultPageViewModel;

            if (returnDuplicatesResult != null)
            {
                byte[] previousHash = null;
                // ToDo: Order By? Verify that this is being ordered earlier in the stack, and move here.
                foreach (ScannedFile scannedFile in returnDuplicatesResult)
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
                    resultPageViewModel.Duplicates.Add(scanResult);
                    previousHash = scannedFile.Hash;
                }
            }

            // End
            App.NavigationService.Navigate(resultPage);
        }
    }
}
