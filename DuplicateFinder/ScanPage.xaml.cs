using DuplicateFinder.Models;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DuplicateFinder
{
    /// <summary>
    /// Interaction logic for ScanPage.xaml
    /// </summary>
    public partial class ScanPage : Page
    {
        private IScanningController _controller;

        public ScanPage(IScanningController controller)
        {
            _controller = controller;
            InitializeComponent();
        }

        public async Task StartScanAsync()
        {
            await _controller.BeginScanAsync(ProgressBar);

            ResultPageModel resultModel = new ResultPageModel();
            resultModel.FoundDuplicates = await _controller.RetrieveDuplicatesAsync();
            ResultPage resultPage = new ResultPage(resultModel);

            this.NavigationService.Navigate(resultPage);
        }
    }
}
