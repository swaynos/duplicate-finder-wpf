using DuplicateFinder.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Forms = System.Windows.Forms;

namespace DuplicateFinder.Tests
{
    [TestClass]
    public class HomePageTest
    {
        [TestMethod]
        public void AddButton_Click_AddsLocationsToPageAndModel()
        {
            // ARRANGE
            string selectedLocation = "C:\\foobar";
            HomePage homePage = new HomePage(GetMockFolderBrowserDialogWrapper(selectedLocation).Object);
            homePage.InitializeComponent();

            // ACT
            homePage.AddButton.RaiseEvent((new RoutedEventArgs(ButtonBase.ClickEvent)));

            // ASSERT
            Assert.AreEqual(selectedLocation, homePage.LocationsListBox.Items[0], "Item does not exist on the page.");
            Assert.AreEqual(selectedLocation, homePage.Model.Locations[0], "Item does not exist in the Model");
        }

        [TestMethod]
        public void AddButton_Click_IgnoresDuplicateLocations()
        {
            // ARRANGE
            string selectedLocation = "C:\\foobar";
            HomePage homePage = new HomePage(GetMockFolderBrowserDialogWrapper(selectedLocation).Object);
            homePage.InitializeComponent();

            // ACT
            homePage.AddButton.RaiseEvent((new RoutedEventArgs(ButtonBase.ClickEvent)));
            homePage.AddButton.RaiseEvent((new RoutedEventArgs(ButtonBase.ClickEvent)));

            // ASSERT
            Assert.AreEqual(1, homePage.LocationsListBox.Items.Count, "Item was added more than once to the page.");
            Assert.AreEqual(1, homePage.Model.Locations.Count, "Item was added more than once to the Model.");
        }

        [TestMethod]
        public void AddButton_Click_NoLocationSelected_DoesNothing()
        {

            // ARRANGE
            string selectedLocation = "C:\\foobar";
            Mock<IFolderBrowserDialogWrapper> folderBrowserDialog = GetMockFolderBrowserDialogWrapper(selectedLocation);
            HomePage homePage = new HomePage(folderBrowserDialog.Object);
            folderBrowserDialog.Setup(t => t.ShowDialogWrapper(It.IsAny<Forms.CommonDialog>())).Returns<Forms.CommonDialog>(t =>
            {
                return Forms.DialogResult.Cancel;
            });
            homePage.InitializeComponent();

            // ACT
            homePage.AddButton.RaiseEvent((new RoutedEventArgs(ButtonBase.ClickEvent)));

            // ASSERT
            Assert.AreEqual(0, homePage.LocationsListBox.Items.Count, "Item was added to the page.");
            Assert.AreEqual(0, homePage.Model.Locations.Count, "Item was added to the Model.");
        }

        [TestMethod]
        public void AddButton_Click_TogglesButtonsWhenLocationsAdded()
        {
            // ARRANGE
            string selectedLocation = "C:\\foobar";
            HomePage homePage = new HomePage(GetMockFolderBrowserDialogWrapper(selectedLocation).Object);
            homePage.InitializeComponent();

            // ACT
            homePage.AddButton.RaiseEvent((new RoutedEventArgs(ButtonBase.ClickEvent)));

            // ASSERT
            Assert.IsTrue(homePage.RemoveButton.IsEnabled, "The RemoveButton is not enabled.");
            Assert.IsTrue(homePage.ScanButton.IsEnabled, "The ScanButton is not enabled.");
        }

        [TestMethod]
        public void RemoveButton_Click_RemovesLocationFromPageAndModel()
        {
            // ARRANGE
            string location = "C:\\foobar";
            HomePage homePage = new HomePage(GetMockFolderBrowserDialogWrapper(string.Empty).Object);
            homePage.LocationsListBox.Items.Add(location);
            homePage.LocationsListBox.SelectedItem = location;
            homePage.Model.Locations.Add(location);

            // ACT
            homePage.RemoveButton.RaiseEvent((new RoutedEventArgs(ButtonBase.ClickEvent)));

            // ASSERT
            Assert.AreEqual(0, homePage.LocationsListBox.Items.Count, "Item was still on the page.");
            Assert.AreEqual(0, homePage.Model.Locations.Count, "Item was still in the Model.");
        }

        [TestMethod]
        public void RemoveButton_Click_NoLocationSelected_DoesNothing()
        {
            // ARRANGE
            string location = "C:\\foobar";
            HomePage homePage = new HomePage(GetMockFolderBrowserDialogWrapper(string.Empty).Object);
            homePage.LocationsListBox.Items.Add(location);
            homePage.Model.Locations.Add(location);

            // ACT
            homePage.RemoveButton.RaiseEvent((new RoutedEventArgs(ButtonBase.ClickEvent)));

            // ASSERT
            Assert.AreEqual(1, homePage.LocationsListBox.Items.Count, "Item was removed from the page.");
            Assert.AreEqual(1, homePage.Model.Locations.Count, "Item was removed from the Model.");
        }

        [TestMethod]
        public void RemoveButton_Click_TogglesButtonsWhenLocationsAdded()
        {
            // ARRANGE
            string location = "C:\\foobar";
            HomePage homePage = new HomePage(GetMockFolderBrowserDialogWrapper(string.Empty).Object);
            homePage.LocationsListBox.Items.Add(location);
            homePage.LocationsListBox.SelectedItem = location;
            homePage.Model.Locations.Add(location);
            homePage.RemoveButton.IsEnabled = true;
            homePage.ScanButton.IsEnabled = true;

            // ACT
            homePage.RemoveButton.RaiseEvent((new RoutedEventArgs(ButtonBase.ClickEvent)));

            // ASSERT
            Assert.IsFalse(homePage.RemoveButton.IsEnabled, "The RemoveButton is enabled.");
            Assert.IsFalse(homePage.ScanButton.IsEnabled, "The ScanButton is enabled.");
        }

        private Mock<IFolderBrowserDialogWrapper> GetMockFolderBrowserDialogWrapper(string selectedPath)
        {
            Mock<IFolderBrowserDialogWrapper> wrapper = new Mock<IFolderBrowserDialogWrapper>();
            Mock<Forms.CommonDialog> dialog = new Mock<Forms.CommonDialog>();
            wrapper.Setup(t => t.GetNewFolderBrowserDialog()).Returns(() =>
            {
                return dialog.Object;
            });
            wrapper.Setup(t => t.ShowDialogWrapper(It.IsAny<Forms.CommonDialog>())).Returns(() =>
            {
                return Forms.DialogResult.OK;
            });
            wrapper.Setup(t => t.GetSelectedPathFromDialog(It.IsAny<Forms.CommonDialog>())).Returns<Forms.CommonDialog>(t =>
            {
                return selectedPath;
            });

            return wrapper;
        }
    }
}
