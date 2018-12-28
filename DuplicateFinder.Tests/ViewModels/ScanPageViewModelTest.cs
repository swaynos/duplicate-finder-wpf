using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DuplicateFinder.Framework;
using DuplicateFinder.Models;
using DuplicateFinder.ViewModels;
using FileHashRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

namespace DuplicateFinder.Tests.ViewModels
{
    [TestClass]
    public class ScanPageViewModelTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            App.NavigationService = new Mock<IPageNavigationService>().Object;
        }

        // Basic end to end test to ensure a Loaded event will eventually fire ScanComplete
        [TestMethod]
        public async Task Loaded_CompletesScan()
        {
            // ARRANGE
            bool scanComplete = false;
            var mockLogger = new Mock<ILogger>();
            var mockScannedFileStore = GetMockScannedFileStore();
            ScanPageViewModel viewModel = new ScanPageViewModel(mockLogger.Object, mockScannedFileStore.Object);
            viewModel.ScanComplete += (s, e) =>
            {
                scanComplete = true;
            };

            // ACT
            await viewModel.Loaded.ExecuteAsync(null);

            // ASSERT
            Assert.IsTrue(scanComplete, "The scan was never completed");
        }

        [TestMethod]
        public async Task BeginScanAsync_CallsPurgeLocationsAsync_CorrectLocations()
        {
            // ARRANGE
            var mockLogger = new Mock<ILogger>();
            var mockScannedFileStore = GetMockScannedFileStore();
            ScanPageViewModel viewModel = new ScanPageViewModel(mockLogger.Object, mockScannedFileStore.Object);
            
            // ACT
            await viewModel.BeginScanAsync();

            // ASSERT
            mockScannedFileStore.Verify(t => t.PurgeLocationsAsync(new List<string>()
            {
                "location1", "purge"
            }), "PurgeLocationsAsync was not called correctly");
        }

        [TestMethod]
        public async Task BeginScanAsync_CallsScanLocationsAsync_CorrectLocations()
        {
            // ARRANGE
            var mockLogger = new Mock<ILogger>();
            var mockScannedFileStore = GetMockScannedFileStore();
            ScanPageViewModel viewModel = new ScanPageViewModel(mockLogger.Object, mockScannedFileStore.Object);
            viewModel.Locations = new List<ScanLocation>()
            {
                new ScanLocation("new")
            };

            // ACT
            await viewModel.BeginScanAsync();

            // ASSERT
            mockScannedFileStore.Verify(t => t.ScanLocationsAsync(new List<string>()
            {
                "new"
            }, It.IsAny<IProgress<int>>()), "ScanLocationsAsync was not called correctly");
        }

        [TestMethod]
        public async Task BeginScanAsync_CallsRescanLocationsAsync_CorrectLocations()
        {
            // ARRANGE
            var mockLogger = new Mock<ILogger>();
            var mockScannedFileStore = GetMockScannedFileStore();
            ScanPageViewModel viewModel = new ScanPageViewModel(mockLogger.Object, mockScannedFileStore.Object);
            viewModel.Locations = new List<ScanLocation>()
            {
                new ScanLocation("location1")
            };

            // ACT
            await viewModel.BeginScanAsync();

            // ASSERT
            mockScannedFileStore.Verify(t => t.RescanLocationsAsync(new List<string>()
            {
                "location1"
            }, It.IsAny<IProgress<int>>()), "ScanLocationsAsync was not called correctly");
        }

        [TestMethod]
        public async Task BeginScanAsync_UpdatesProgress()
        {
            // ARRANGE
            var mockLogger = new Mock<ILogger>();
            var mockScannedFileStore = GetMockScannedFileStore();
            ScanPageViewModel viewModel = new ScanPageViewModel(mockLogger.Object, mockScannedFileStore.Object);

            // ACT
            await viewModel.BeginScanAsync();

            // ASSERT
            Assert.AreNotEqual(0, viewModel.Progress, "The progress is still at 0");
        }

        [TestMethod]
        public async Task BeginScanAsync_InnerException_IsHandled()
        {
            // ARRANGE
            var mockLogger = new Mock<ILogger>();
            var mockScannedFileStore = GetMockScannedFileStore();
            ScanPageViewModel viewModel = new ScanPageViewModel(mockLogger.Object, mockScannedFileStore.Object);
            mockScannedFileStore.Setup(t => t.ListScannedLocationsAsync()).ReturnsAsync(() =>
            {
                throw new NotImplementedException();
            });
            
            // ACT
            await viewModel.BeginScanAsync();

            // ASSERT
            mockLogger.Verify(t => t.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once, "An error was never logged");
        }

        [TestMethod]
        public async Task RetrieveDuplicatesAsync_GetsResultFromScannedFileStore()
        {
            var mockLogger = new Mock<ILogger>();
            var mockScannedFileStore = GetMockScannedFileStore();
            ScanPageViewModel viewModel = new ScanPageViewModel(mockLogger.Object, mockScannedFileStore.Object);
            mockScannedFileStore.Setup(t => t.ListDuplicateFilesAsync()).ReturnsAsync(() =>
            {
                return new List<ScannedFile>()
                {
                    new ScannedFile()
                };
            });

            // ACT
            List<ScannedFile> results = await viewModel.RetrieveDuplicatesAsync();

            // ASSERT
            Assert.AreEqual(1, results.Count, "The wrong number of items were returned");
        }

        [TestMethod]
        public async Task RetrieveDuplicatesAsync_InnerException_IsHandled()
        {
            var mockLogger = new Mock<ILogger>();
            var mockScannedFileStore = GetMockScannedFileStore();
            ScanPageViewModel viewModel = new ScanPageViewModel(mockLogger.Object, mockScannedFileStore.Object);
            mockScannedFileStore.Setup(t => t.ListDuplicateFilesAsync()).ReturnsAsync(() =>
            {
                throw new NotImplementedException();
            });

            // ACT
            List<ScannedFile> results = await viewModel.RetrieveDuplicatesAsync();

            // ASSERT
            Assert.AreEqual(0, results.Count, "The wrong number of items were returned");
            mockLogger.Verify(t => t.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once, "An error was never logged");
        }

        [TestMethod]
        public async Task OnLoaded_NavigatesPage()
        {
            // ARRANGE
            var mockLogger = new Mock<ILogger>();
            var mockScannedFileStore = GetMockScannedFileStore();
            var mockNavigationService = new Mock<IPageNavigationService>();
            App.NavigationService = mockNavigationService.Object;
            ScanPageViewModel viewModel = new ScanPageViewModel(mockLogger.Object, mockScannedFileStore.Object);

            // ACT
            await viewModel.OnLoaded();

            // ASSERT
            mockNavigationService.Verify(t => t.Navigate(It.IsAny<object>()), "The NavigationService failed to navigate anywhere");
        }

        private Mock<IScannedFileStore> GetMockScannedFileStore()
        {
            Mock<IScannedFileStore> scannedFileStore = new Mock<IScannedFileStore>();
            scannedFileStore.Setup(t => t.ListScannedLocationsAsync()).ReturnsAsync(() =>
            {
                return new List<string>()
                {
                    "location1", "purge"
                };
            });
            scannedFileStore.Setup(t => t.ListDuplicateFilesAsync()).ReturnsAsync(() =>
            {
                return new List<ScannedFile>();
            });
            return scannedFileStore;
        }
    }
}
