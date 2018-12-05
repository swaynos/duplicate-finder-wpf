using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DuplicateFinder
{
    /// <summary>
    /// Interaction logic for ResultPage.xaml
    /// </summary>
    public partial class ResultPage : Page
    {
        private IRecycleFile _recycleFile;

        ResultPageModel Model { get; set; }

        internal ResultPage(IRecycleFile recycleFile, ResultPageModel model)
        {
            _recycleFile = recycleFile;
            Model = model;

            InitializeComponent();

            foreach (string duplicateFile in model.FlattenFoundDuplicates())
            {
                DuplicatesListBox.Items.Add(duplicateFile);
            }
        }

        public ResultPage(ResultPageModel model) : this(new RecycleFile(), model)
        {
        }

        public void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (string file in DuplicatesListBox.SelectedItems)
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = file;
                process.Start();
            }
        }

        public async void RecycleButton_Click(object sender, RoutedEventArgs e)
        {
            bool suppressRecycleFileDialog = Properties.Settings.Default.SuppressRecycleFileDialog;
            List<string> recycledItems = new List<string>();

            foreach (string itemToRecycle in DuplicatesListBox.SelectedItems)
            {
                if (await _recycleFile.RecycleAsync(itemToRecycle, suppressRecycleFileDialog))
                {
                    recycledItems.Add(itemToRecycle);
                }
            }

            foreach (string recycledItem in recycledItems)
            {
                Model.RemoveFileModelFromModel(recycledItem);
                DuplicatesListBox.Items.RemoveAt(DuplicatesListBox.Items.IndexOf(recycledItem));
            }
        }

        public void DuplicatesListBox_Changed(object sender, RoutedEventArgs e)
        {
            bool isCountValid = DuplicatesListBox.Items.Count > 0;
            if (isCountValid)
            {
                if (!RecycleButton.IsEnabled)
                {
                    RecycleButton.IsEnabled = true;
                }
                if (!PreviewButton.IsEnabled)
                {
                    PreviewButton.IsEnabled = true;
                }
            }
        }

        public void FilterBox_TextChanged(object sender, RoutedEventArgs e)
        {
            DuplicatesListBox.Items.Clear();

            foreach(string duplicateFile in Model.FlattenFoundDuplicates())
            {
                if (duplicateFile.Contains(FilterBox.Text))
                {
                    DuplicatesListBox.Items.Add(duplicateFile);
                }
            }
        }
    }
}
