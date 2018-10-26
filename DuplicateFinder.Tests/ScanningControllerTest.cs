using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using DuplicateFinder.Models;
using FileHashRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using NLog;

namespace DuplicateFinder.Tests
{
    [TestClass]
    public class ScanningControllerTest
    {
        [TestMethod]
        public async Task BeginScanAsync_InvokesScannedFileStoreMethods()
        {
            // ARRANGE
            List<double> addedValues = new List<double>();
            List<string> locations = new List<string>()
            {
                "location1", "location2"
            };
            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IScannedFileStore> scannedFileStore = new Mock<IScannedFileStore>();
            ScanningController controller = new ScanningController(locations, scannedFileStore.Object, logger.Object);
            Mock<ProgressBar> control = new Mock<ProgressBar>();
            scannedFileStore.Setup(t => t.ListScannedLocationsAsync()).ReturnsAsync(() =>
            {
                return new List<string>()
                {
                    "location1", "purge"
                };
            });

            // ACT
            await controller.BeginScanAsync(control.Object);
            
            // ASSERT
            scannedFileStore.Verify(t => t.PurgeLocationsAsync(It.Is<List<string>>(pL => pL[0].Equals("purge"))), Times.Once());
            scannedFileStore.Verify(t => t.ScanLocationsAsync(It.Is<List<string>>(sL => sL[0].Equals("location2")), 
                It.IsAny<IProgress<int>>()), Times.Once, "The expected location was never scanned");
            scannedFileStore.Verify(t => t.RescanLocationsAsync(It.Is<List<string>>(rL => rL[0].Equals("location1")), 
                It.IsAny<IProgress<int>>()), Times.Once, "The expected location was never rescanned");
        }

        [TestMethod]
        public async Task RetrieveDuplicatesAsync_CollapsesEqualScannedFiles()
        {
            // ARRANGE
            Mock<IScannedFileStore> scannedFileStore = new Mock<IScannedFileStore>();
            ScanningController controller = new ScanningController(null, scannedFileStore.Object, null);
            scannedFileStore.Setup(t => t.ListDuplicateFilesAsync()).ReturnsAsync(() =>
            {
                return new List<ScannedFile>()
                {
                    new ScannedFile()
                    {
                        Id = 1,
                        Hash = new byte[32],
                        Name = "Foo.png",
                        Path = "X:\\Foo\\Foo.png"
                    },
                    new ScannedFile()
                    {
                        Id = 2,
                        Hash = new byte[32],
                        Name = "Bar.png",
                        Path = "X:\\Bar\\Bar.png"
                    },
                    new ScannedFile()
                    {
                        Id = 3,
                        Hash = new byte[32],
                        Name = "FooBar.png",
                        Path = "X:\\FooBar.png"
                    }
                };
            });

            // ACT
            List<DuplicateResultModel> results = await controller.RetrieveDuplicatesAsync();

            // ASSERT
            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public async Task BeginScanAsync_ExceptionsLogged()
        {
            // ARRANGE
            List<string> locations = new List<string>()
            {
                "location1", "location2"
            };
            List<string> logs = new List<string>();
            Mock<ILogger> logger = new Mock<ILogger>();
            logger.Setup(t => t.Error(It.IsAny<Exception>(), It.IsAny<string>())).Callback<Exception, string>((t, q) =>
            {
                logs.Add(q);
            });
            Mock<IScannedFileStore> scannedFileStore = new Mock<IScannedFileStore>();
            ScanningController controller = new ScanningController(locations, scannedFileStore.Object, logger.Object);
            Mock<ProgressBar> control = new Mock<ProgressBar>();
            scannedFileStore.Setup(t => t.ListScannedLocationsAsync()).ReturnsAsync(() =>
            {
                throw new Exception("test");
            });

            // ACT
            await controller.BeginScanAsync(control.Object);

            // ASSERT
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public async Task RetrieveDuplicatesAsync_ExceptionsLogged()
        {
            // ARRANGE
            List<string> locations = new List<string>()
            {
                "location1", "location2"
            };
            List<string> logs = new List<string>();
            Mock<ILogger> logger = new Mock<ILogger>();
            logger.Setup(t => t.Error(It.IsAny<Exception>(), It.IsAny<string>())).Callback<Exception, string>((t, q) =>
            {
                logs.Add(q);
            });
            Mock<IScannedFileStore> scannedFileStore = new Mock<IScannedFileStore>();
            ScanningController controller = new ScanningController(locations, scannedFileStore.Object, logger.Object);
            Mock<ProgressBar> control = new Mock<ProgressBar>();
            scannedFileStore.Setup(t => t.ListDuplicateFilesAsync()).ReturnsAsync(() =>
            {
                throw new Exception("test");
            });

            // ACT
            await controller.RetrieveDuplicatesAsync();

            // ASSERT
            Assert.AreEqual(1, logs.Count);
        }
    }
}
