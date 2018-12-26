using DuplicateFinder.Framework;
using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using DuplicateFinder.Views;
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
        public ObservableCollection<ScanLocation> Locations { get; set; }

        public int Progress { get; set; }

        public IAsyncCommand LoadCommand { get; private set; }

        //ToDo: Unit Test this Event
        public event EventHandler ScanComplete;

        protected void OnScanComplete()
        {
            ScanComplete?.Invoke(this, EventArgs.Empty);
        }

        public ScanPageViewModel()
        {
            this.Locations = new ObservableCollection<ScanLocation>();
            this.Progress = 0;
            ScanComplete += (sender, e) =>
            {
                ShowResultPage();
            };
            LoadCommand = AsyncCommand.Create(() => BeginScanAsync());
        }

        public async Task BeginScanAsync()
        {
            // TODo: Implement the async Scan
            OnScanComplete();
        }

        private void ShowResultPage()
        {
            ResultPage reultPage = new ResultPage();
            ResultPageViewModel resultPageViewModel = reultPage.DataContext as ResultPageViewModel;
            // ToDo: give models to resultPageViewModel
            App.NavigationService.Navigate(reultPage);
        }
    }
}
