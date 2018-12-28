using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using DuplicateFinder.ViewModels;
using FileHashRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

namespace DuplicateFinder.Tests.ViewModels
{
    [TestClass]
    public class ResultPageViewModelTest
    {
        [TestMethod]
        public async Task Recycle_RecyclesFile()
        {
            // ARRANGE
            string filePath = "C:\\Foo\\foo.txt";
            var mockLogger = new Mock<ILogger>();
            var mockProcess = new Mock<IProcess>();
            var mockRecycleFile = GetMockRecycleFile();
            ResultPageViewModel viewModel = new ResultPageViewModel(mockLogger.Object, mockProcess.Object, mockRecycleFile.Object);
            viewModel.Duplicates.Add(new ScanResult()
            {
                FilePath = filePath,
                Hash = new byte[32],
                IsSelected = true
            });

            // ACT
            await viewModel.Recycle.ExecuteAsync(null);

            // ASSERT
            mockRecycleFile.Verify(t => t.RecycleAsync(filePath, It.IsAny<bool>()), "The RecycleAsync operation was never called.");
            Assert.AreEqual(0, viewModel.Duplicates.Count, "The file was not removed from the collection");

        }

        [TestMethod]
        public async Task Reycle_NoSelection_DoesNothing()
        {
            // ARRANGE
            string filePath = "C:\\Foo\\foo.txt";
            var mockLogger = new Mock<ILogger>();
            var mockProcess = new Mock<IProcess>();
            var mockRecycleFile = GetMockRecycleFile();
            ResultPageViewModel viewModel = new ResultPageViewModel(mockLogger.Object, mockProcess.Object, mockRecycleFile.Object);
            viewModel.Duplicates.Add(new ScanResult()
            {
                FilePath = filePath,
                Hash = new byte[32],
                IsSelected = false
            });

            // ACT
            await viewModel.Recycle.ExecuteAsync(null);

            // ASSERT
            mockRecycleFile.Verify(t => t.RecycleAsync(filePath, It.IsAny<bool>()), Times.Never, "The RecycleAsync operation was called.");
            Assert.AreEqual(1, viewModel.Duplicates.Count, "The file was removed from the collection");
        }

        [TestMethod]
        public async Task Preview_PreviewsFile()
        {
            // ARRANGE
            string filePath = "C:\\Foo\\foo.txt";
            var mockLogger = new Mock<ILogger>();
            var mockProcess = new Mock<IProcess>();
            var mockRecycleFile = GetMockRecycleFile();
            ResultPageViewModel viewModel = new ResultPageViewModel(mockLogger.Object, mockProcess.Object, mockRecycleFile.Object);
            viewModel.Duplicates.Add(new ScanResult()
            {
                FilePath = filePath,
                Hash = new byte[32],
                IsSelected = true
            });

            // ACT
            await viewModel.Preview.ExecuteAsync(null);

            // ASSERT
            mockProcess.Verify(t => t.StartAsync(filePath), "The IProcess.StartAsync operation was never called.");
        }

        [TestMethod]
        public async Task Preview_NoSelection_DoesNothing()
        {
            // ARRANGE
            string filePath = "C:\\Foo\\foo.txt";
            var mockLogger = new Mock<ILogger>();
            var mockProcess = new Mock<IProcess>();
            var mockRecycleFile = GetMockRecycleFile();
            ResultPageViewModel viewModel = new ResultPageViewModel(mockLogger.Object, mockProcess.Object, mockRecycleFile.Object);
            viewModel.Duplicates.Add(new ScanResult()
            {
                FilePath = filePath,
                Hash = new byte[32],
                IsSelected = false
            });

            // ACT
            await viewModel.Preview.ExecuteAsync(null);

            // ASSERT
            mockProcess.Verify(t => t.StartAsync(filePath), Times.Never, "The IProcess.StartAsync operation was called when it was expected to not.");
        }

        [TestMethod]
        public void AddScannedFiles_AddsFileToModel()
        {
            // ARRANGE
            var mockLogger = new Mock<ILogger>();
            var mockProcess = new Mock<IProcess>();
            var mockRecycleFile = GetMockRecycleFile();
            ResultPageViewModel viewModel = new ResultPageViewModel(mockLogger.Object, mockProcess.Object, mockRecycleFile.Object);
            List<ScannedFile> scannedFiles = GetScannedFiles();

            // ACT
            viewModel.AddScannedFiles(scannedFiles);

            // ASSERT
            Assert.AreEqual(scannedFiles.Count, viewModel.Duplicates.Count, "The wrong number of items were added to the view model");
        }

        [TestMethod]
        public void AddScannedFiles_AddsAlternatingColors()
        {
            // ARRANGE
            var mockLogger = new Mock<ILogger>();
            var mockProcess = new Mock<IProcess>();
            var mockRecycleFile = GetMockRecycleFile();
            ResultPageViewModel viewModel = new ResultPageViewModel(mockLogger.Object, mockProcess.Object, mockRecycleFile.Object);
            List<ScannedFile> scannedFiles = GetScannedFiles();

            // ACT
            viewModel.AddScannedFiles(scannedFiles);

            // ASSERT
            string expectedColor1 = BackgroundColor.Transparent.ToString();
            string expectedColor2 = BackgroundColor.Grey.ToString();
            string failureMessage = "The background color does not match what was expected";
            Assert.AreEqual(expectedColor1, viewModel.Duplicates[0].Background, failureMessage);
            Assert.AreEqual(expectedColor1, viewModel.Duplicates[1].Background, failureMessage);
            Assert.AreEqual(expectedColor2, viewModel.Duplicates[2].Background, failureMessage);
            Assert.AreEqual(expectedColor2, viewModel.Duplicates[3].Background, failureMessage);
            Assert.AreEqual(expectedColor1, viewModel.Duplicates[4].Background, failureMessage);
            Assert.AreEqual(expectedColor1, viewModel.Duplicates[5].Background, failureMessage);
        }

        [TestMethod]
        public void Loaded_WithData_TogglesButtons()
        {
            // ARRANGE
            ResultPageViewModel viewModel = new ResultPageViewModel();
            viewModel.Duplicates.Add(new ScanResult()
            {
                FilePath = "C:\\Foo\\foo.txt",
                Hash = new byte[32],
                IsSelected = false
            });

            // ACT
            viewModel.Loaded.Execute(null);

            // ASSERT
            Assert.IsTrue(viewModel.Preview.CanExecute(null), "Preview is not enabled");
            Assert.IsTrue(viewModel.Recycle.CanExecute(null), "Recycle is not enabled");
        }

        private Mock<IRecycleFile> GetMockRecycleFile()
        {
            Mock<IRecycleFile> mockRecycleFile = new Mock<IRecycleFile>();
            mockRecycleFile.Setup(t => t.RecycleAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync((string f, bool s) =>
            {
                return true;
            });
            return mockRecycleFile;
        }

        /// <summary>
        /// Helper method will return two sets of duplicate ScannedFiles
        /// </summary>
        /// <returns>New List of ScannedFiles</returns>
        private List<ScannedFile> GetScannedFiles()
        {
            Func<int, string, byte[], ScannedFile> createScannedFile = (id, name, hash) =>
            {
                return new ScannedFile()
                {
                    Id = id,
                    Name = name,
                    Path = string.Format("C:\\foo\\{0}", name),
                    Hash = hash
                };
            };
            // Helper delegate to get a new 32 byte hash by only providing the first byte
            Func<byte, byte[]> getNewHash = (f) =>
            {
                byte[] newHash = new byte[32];
                newHash[0] = f;
                return newHash;
            };

            List<ScannedFile> scannedFiles = new List<ScannedFile>();
            scannedFiles.Add(createScannedFile(1, "foo1.txt", getNewHash(0x01)));
            scannedFiles.Add(createScannedFile(2, "foo2.txt", getNewHash(0x01)));
            scannedFiles.Add(createScannedFile(3, "bar1.txt", getNewHash(0x02)));
            scannedFiles.Add(createScannedFile(4, "bar2.txt", getNewHash(0x02)));
            scannedFiles.Add(createScannedFile(5, "foobar1.txt", getNewHash(0x03)));
            scannedFiles.Add(createScannedFile(6, "foobar2.txt", getNewHash(0x03)));

            return scannedFiles;
        }
    }
}
