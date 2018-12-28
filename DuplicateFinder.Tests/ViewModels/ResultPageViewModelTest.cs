using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using DuplicateFinder.ViewModels;
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
    }
}
