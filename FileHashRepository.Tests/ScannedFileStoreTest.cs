using FileHashRepository.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;

namespace FileHashRepository.Tests
{
    [TestClass]
    public class ScannedFileStoreTest
    {
        [TestMethod]
        public async Task ListScannedLocationsAsync_ReturnsListFromService()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);

            // ACT
            List<string> results = await scannedFileStore.ListScannedLocationsAsync();

            // ASSERT
            service.Verify(t => t.ListScannedLocationsAsync(), Times.Once());
        }

        [TestMethod]
        public async Task ScanLocationAsync_StoresScannedFilesFromDirectory()
        {
            // ARRANGE
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foobar\file1.txt", new MockFileData("This is a test file."));
            dictionaryMockFileData.Add(@"C:\foobar\file2.txt", new MockFileData("This is another test file."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };

            // ACT
            await scannedFileStore.ScanLocationsAsync(locations, new MockScannedFileStoreProgress());

            // ASSERT
            service.Verify(t => t.InsertScannedFileAsync(It.Is<ScannedFile>(sf => sf.Path.Equals(@"C:\foobar\file1.txt"))), Times.Once);
            service.Verify(t => t.InsertScannedFileAsync(It.Is<ScannedFile>(sf => sf.Path.Equals(@"C:\foobar\file2.txt"))), Times.Once);
        }

        [TestMethod]
        public async Task ScanLocationAsync_ReportsProgressCorrectly()
        {
            // ARRANGE
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            Action<int> addFileDelegate = (int id) =>
            {
                string path = string.Format(@"C:\foobar\file{0}.txt", id);
                string content = string.Format("This is a test file{0}.", id);
                dictionaryMockFileData.Add(path, new MockFileData(content));
            };
            for (int i = 0; i < 100; i ++)
            {
                addFileDelegate(i);
            }
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };

            // ACT
            await scannedFileStore.ScanLocationsAsync(locations, progress);

            // ASSERT
            Assert.AreEqual(progress.ReportedValues.Count, 100);
            Assert.AreEqual(progress.ReportedValues[49], 50);
            Assert.AreEqual(progress.ReportedValues[99], 100);
        }

        [TestMethod]
        public async Task ScanLocationAsync_StoresScannedLocation()
        {
            // ARRANGE
            List<ScannedLocation> scannedLocations = new List<ScannedLocation>();
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foobar\file.txt", new MockFileData("This is a test file{0}."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };
            service.Setup(t => t.InsertScannedLocationAsync(It.IsAny<ScannedLocation>()))
                .Returns(Task.CompletedTask)
                .Callback((ScannedLocation s) => { scannedLocations.Add(s); });

            // ACT
            await scannedFileStore.ScanLocationsAsync(locations, progress);

            // ASSERT
            Assert.AreEqual(locations[0], scannedLocations[0].Path);
        }

        [TestMethod]
        public async Task ScanLocationAsync_ExistingScanedLocationsPurged()
        {
            // ARRANGE
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foobar\file.txt", new MockFileData("This is a test file{0}."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };

            // ACT
            await scannedFileStore.ScanLocationsAsync(locations, progress);

            // ASSERT
            service.Verify(t => t.PurgeScannedLocationsAsync(It.IsAny<List<string>>()));
        }

        [TestMethod]
        public async Task RescanLocationAsync_StoresAddedScannedFilesFromDiretory()
        {
            // ARRANGE
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foobar\file1.txt", new MockFileData("This is a test file."));
            dictionaryMockFileData.Add(@"C:\foobar\file2.txt", new MockFileData("This is another test file."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };
            service.Setup(t => t.ListScannedFilePathsAsync(It.IsAny<List<string>>())).ReturnsAsync((List<string> locs) =>
            {
                return new List<string>()
                {
                     @"C:\foobar\foo.bar"

                };
            });

            // ACT
            await scannedFileStore.RescanLocationsAsync(locations, progress);

            // ASSERT
            service.Verify(t => t.InsertScannedFileAsync(It.IsAny<ScannedFile>()), Times.Exactly(2), "The correct number of files were not inserted.");
        }

        [TestMethod]
        public async Task RescanLocationAsync_RemovesScannedFilesNotFoundInDiretory()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(@"C:\foobar");
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };
            service.Setup(t => t.ListScannedFilePathsAsync(It.IsAny<List<string>>())).ReturnsAsync((List<string> locs) =>
            {
                return new List<string>()
                {
                     @"C:\foobar\foo.bar"

                };
            });

            // ACT
            await scannedFileStore.RescanLocationsAsync(locations, progress);

            // ASSERT
            service.Verify(t => t.RemoveScannedFilesByFilePathAsync(It.Is<string>(s => s.Equals(@"C:\foobar\foo.bar"))));
        }

        [TestMethod]
        public async Task RescanLocationAsync_ReportsProgressCorrectly()
        {
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<ScannedFile> scannedFiles = new List<ScannedFile>();
            Action<int> addFileDelegate = (int id) =>
            {
                string path = string.Format(@"C:\foobar\file{0}.txt", id);
                string content = string.Format("This is a test file{0}.", id);
                fileSystem.AddFile(path, new MockFileData(content));
            };
            Action<int> addFileRemovalDelegate = (int id) =>
            {
                string path = string.Format(@"C:\foobar\oldfile{0}.txt", id);
                string name = string.Format("oldfile{0}.txt", id);
                byte[] hash = new byte[32];
                hash[0] = System.Convert.ToByte(id);
                scannedFiles.Add(new ScannedFile()
                {
                    Path = path,
                    Name = name,
                    Hash = hash

                });
            };
            for (int i = 0; i < 50; i++)
            {
                addFileDelegate(i);
                addFileRemovalDelegate(i);
            }
            service.Setup(t => t.ListScannedFilePathsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync((List<string> locs) =>
                {
                    return scannedFiles.Select(t => t.Path).ToList();
                });
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };

            // ACT
            await scannedFileStore.RescanLocationsAsync(locations, progress);

            // ASSERT
            Assert.AreEqual(progress.ReportedValues.Count, 100);
            Assert.AreEqual(progress.ReportedValues[49], 50);
            Assert.AreEqual(progress.ReportedValues[99], 100);
        }

        [TestMethod]
        public async Task RescanLocationAsync_StoresScannedLocation()
        {
            // ARRANGE
            List<ScannedLocation> scannedLocations = new List<ScannedLocation>();
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foobar\file.txt", new MockFileData("This is a test file{0}."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };
            service.Setup(t => t.ListScannedFilePathsAsync(It.IsAny<List<string>>())).ReturnsAsync((List<string> locs) =>
            {
                return new List<string>()
                {
                     @"C:\foobar\foo.bar"

                };
            });
            service.Setup(t => t.InsertScannedLocationAsync(It.IsAny<ScannedLocation>()))
                .Returns(Task.CompletedTask)
                .Callback((ScannedLocation s) => { scannedLocations.Add(s); });

            // ACT
            await scannedFileStore.RescanLocationsAsync(locations, progress);

            // ASSERT
            service.Verify(t => t.InsertScannedLocationAsync(It.Is<ScannedLocation>(sl => sl.Path.Equals(@"C:\foobar"))));
        }

        [TestMethod]
        public async Task ListDuplicateFiles_ReturnsDuplicatesFromService()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);

            // ACT
            List<ScannedFile> results = await scannedFileStore.ListDuplicateFilesAsync();

            // ASSERT
            service.Verify(t => t.ReturnDuplicatesAsync(), Times.Once);
        }

        /// <summary>
        /// This test is the scenario that presented in a Bug in which the
        /// Rescan would purge the entries added during Scan. This function 
        /// does a lot (probably too much) so I decided to make it an end to
        /// end test.
        /// </summary>
        [TestMethod]

        public async Task RescanLocationAsync_PreviouslyScannedFilesPersist()
        {
            // ARRANGE
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foobar\file1.txt", new MockFileData("This is a test file."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            List<ScannedFile> scannedFiles = new List<ScannedFile>()
            {
                new ScannedFile()
                {
                    Name = "file1.txt",
                    Path = @"C:\foobar\file1.txt",
                    Hash = new byte[32]
                },
                new ScannedFile()
                {
                    Name = "file2.txt",
                    Path = @"C:\foobar\file2.txt",
                    Hash = new byte[32]
                }
            };
            List<ScannedLocation> scannedLocations = new List<ScannedLocation>()
            {
                new ScannedLocation()
                {
                    Path = @"C:\foobar"
                }
            };
            IDataCache<ScannedFile> scannedFileDataCache = new DataCache<ScannedFile>(scannedFiles);
            IFileHashService service = new FileHashService(
                scannedFileDataCache, 
                new DataCache<ScannedLocation>(scannedLocations));
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service);


            // ACT
            await scannedFileStore.RescanLocationsAsync(new List<string>
            {
                @"C:\foobar"
            }, new MockScannedFileStoreProgress());

            // ASSERT
            var result = scannedFileDataCache.ListData().ToList();
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].Path.Equals(@"C:\foobar\file1.txt"));
        }

        [TestMethod]
        public async Task PurgeLocationsAsync_CallsFileHashServicePurgesLocations()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            List<string> purgeLocations = new List<string>()
            {
                @"C:\foobar"
            };

            // ACT
            await scannedFileStore.PurgeLocationsAsync(purgeLocations);

            // ASSERT
            service.Verify(t => t.PurgeScannedLocationsAsync(purgeLocations), Times.Once());
        }

        [TestMethod]
        public async Task ScanFile_InsertsScannedFile()
        {
            // ARRANGE
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foo\bar.txt", new MockFileData("This is a test file."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);

            // ACT
            await scannedFileStore.ScanFile(@"C:\foo\bar.txt", null, 0, 0);

            // ASSERT
            service.Verify(t => t.InsertScannedFileAsync(It.IsAny<ScannedFile>()), Times.Once());
        }

        [TestMethod]
        public async Task ScanFile_UpdatesProgress()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();

            // ACT
            await scannedFileStore.ScanFile(@"C:\foo\bar.txt", progress, 1, 10);

            // ASSERT
            Assert.AreEqual(10, progress.ReportedValues[0]);
        }

        [TestMethod]
        public async Task RemoveFile_RemovesScannedFiles()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);

            // ACT
            await scannedFileStore.RemoveFile(@"C:\foo\bar.txt", null, 0, 0);

            // ASSERT
            service.Verify(t => t.RemoveScannedFilesByFilePathAsync(It.Is<string>(s => s.Equals(@"C:\foo\bar.txt"))));
        }

        [TestMethod]
        public async Task RemoveFile_UpdatesProgress()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            Mock<IFileHashService> service = new Mock<IFileHashService>();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, service.Object);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            //factory.ScannedFiles.Add(new ScannedFile()
            //{
            //    Hash = new byte[32],
            //    Path = @"C:\foo\bar.txt",
            //    Name = "bar.txt"
            //});
            // ToDo: Fix

            // ACT
            await scannedFileStore.RemoveFile(@"C:\foo\bar.txt", progress, 1, 1);

            // ASSERT
            Assert.AreEqual(100, progress.ReportedValues[0]);
        }
    }
}
