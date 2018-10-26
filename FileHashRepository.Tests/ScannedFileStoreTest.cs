using FileHashRepository.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
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
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);

            // ACT
            List<string> results = await scannedFileStore.ListScannedLocationsAsync();

            // ASSERT
            factory.MockFileHashService.Verify(t => t.ListScannedLocationsAsync(), Times.Once());
        }

        [TestMethod]
        public async Task ScanLocationAsync_StoresScannedFilesFromDirectory()
        {
            // ARRANGE
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foobar\file1.txt", new MockFileData("This is a test file."));
            dictionaryMockFileData.Add(@"C:\foobar\file2.txt", new MockFileData("This is another test file."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };

            // ACT
            await scannedFileStore.ScanLocationsAsync(locations, new MockScannedFileStoreProgress());

            // ASSERT
            Assert.AreEqual(factory.ScannedFiles.Count, 2);
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
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
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
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };
            factory.MockFileHashService.Setup(t => t.InsertScannedLocationAsync(It.IsAny<ScannedLocation>()))
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
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };

            // ACT
            await scannedFileStore.ScanLocationsAsync(locations, progress);

            // ASSERT
            factory.MockFileHashService.Verify(t => t.PurgeScannedLocationsAsync(It.IsAny<List<string>>()));
        }

        [TestMethod]
        public async Task RescanLocationAsync_StoresAddedScannedFilesFromDiretory()
        {
            // ARRANGE
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foobar\file1.txt", new MockFileData("This is a test file."));
            dictionaryMockFileData.Add(@"C:\foobar\file2.txt", new MockFileData("This is another test file."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };

            // ACT
            await scannedFileStore.RescanLocationsAsync(locations, progress);

            // ASSERT
            Assert.AreEqual(factory.ScannedFiles.Count, 2);
        }

        [TestMethod]
        public async Task RescanLocationAsync_RemovesScannedFilesRemovedFilesFromDiretory()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(@"C:\foobar");
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            factory.ScannedFiles.Add(new ScannedFile()
            {
                Id = 1,
                Name = "foo.bar",
                Path = @"C:\foobar\foo.bar",
                Hash = new byte[32]
            });
            factory.ScannedFiles.Add(new ScannedFile()
            {
                Id = 2,
                Name = "foobar.foo",
                Path = @"C:\foobar\foobar.foo",
                Hash = new byte[32]
            });
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };

            // ACT
            await scannedFileStore.RescanLocationsAsync(locations, progress);

            // ASSERT
            Assert.AreEqual(factory.ScannedFiles.Count, 0);
        }

        [TestMethod]
        public async Task RescanLocationAsync_ReportsProgressCorrectly()
        {
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
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
                factory.ScannedFiles.Add(new ScannedFile()
                {
                    Id = id,
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
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            List<string> locations = new List<string>()
            {
                @"C:\foobar"
            };
            factory.MockFileHashService.Setup(t => t.InsertScannedLocationAsync(It.IsAny<ScannedLocation>()))
                .Returns(Task.CompletedTask)
                .Callback((ScannedLocation s) => { scannedLocations.Add(s); });

            // ACT
            await scannedFileStore.RescanLocationsAsync(locations, progress);

            // ASSERT
            Assert.AreEqual(locations[0], scannedLocations[0].Path);
        }

        [TestMethod]
        public async Task ListDuplicateFiles_ReturnsResultsFromService()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            factory.ScannedFiles = new List<ScannedFile>()
            {
                new ScannedFile()
                {
                    Hash = new byte[32],
                    Id = 1,
                    Name = "FooBar.png",
                    Path = "X:\\Foo\\Bar\\FooBar.png"
                }
            };

            // ACT
            List<ScannedFile> results = await scannedFileStore.ListDuplicateFilesAsync();

            // ASSERT
            Assert.IsTrue(results[0].Name.Equals("FooBar.png"));
        }

        /// <summary>
        /// This test is the scenario that presented in a Bug in which the
        /// Rescan would purge the entries added during Scan.
        /// </summary>
        [TestMethod]
        public async Task RescanLocationAsync_PreviouslyScannedFilesPersist()
        {
            // ARRANGE
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foobar\file1.txt", new MockFileData("This is a test file."));
            dictionaryMockFileData.Add(@"C:\foobar\file2.txt", new MockFileData("This is another test file."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            List<string> newLocations = new List<string>()
            {
                @"C:\foobar"
            };
            List<string> previousLocations = new List<string>();
            await scannedFileStore.ScanLocationsAsync(newLocations, new MockScannedFileStoreProgress());

            // ACT
            await scannedFileStore.RescanLocationsAsync(previousLocations, new MockScannedFileStoreProgress());

            // ASSERT
            Assert.AreEqual(factory.ScannedFiles.Count, 2);
        }

        [TestMethod]
        public async Task PurgeLocationsAsync_CallsFileHashServicePurgesLocations()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            List<string> purgeLocations = new List<string>()
            {
                @"C:\foobar"
            };
            factory.ScannedFiles.Add(new ScannedFile()
            {
                Id = 1,
                Name = "foo.bar",
                Path = @"C:\foobar\foo.bar",
                Hash = new byte[32]
            });
            factory.ScannedFiles.Add(new ScannedFile()
            {
                Id = 2,
                Name = "foobar.foo",
                Path = @"C:\foo\foobar.foo",
                Hash = new byte[32]
            });

            // ACT
            await scannedFileStore.PurgeLocationsAsync(purgeLocations);

            // ASSERT
            factory.MockFileHashService.Verify(t => t.PurgeScannedLocationsAsync(purgeLocations), Times.Once());
        }

        [TestMethod]
        public async Task ScanFile_InsertsScannedFile()
        {
            // ARRANGE
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();
            dictionaryMockFileData.Add(@"C:\foo\bar.txt", new MockFileData("This is a test file."));
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);

            // ACT
            await scannedFileStore.ScanFile(@"C:\foo\bar.txt", null, 0, 0);

            // ASSERT
            Assert.AreEqual(1, factory.ScannedFiles.Count);
        }

        [TestMethod]
        public async Task ScanFile_UpdatesProgress()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
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
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            factory.ScannedFiles.Add(new ScannedFile()
            {
                Id = 1,
                Hash = new byte[32],
                Path = @"C:\foo\bar.txt",
                Name = "bar.txt"
            });
            factory.ScannedFiles.Add(new ScannedFile()
            {
                Id = 2,
                Hash = new byte[32],
                Path = @"C:\foo\bar.txt",
                Name = "bar.txt"
            });

            // ACT
            await scannedFileStore.RemoveFile(@"C:\foo\bar.txt", null, 0, 0);

            // ASSERT
            Assert.AreEqual(0, factory.ScannedFiles.Count);
        }

        [TestMethod]
        public async Task RemoveFile_UpdatesProgress()
        {
            // ARRANGE
            MockFileSystem fileSystem = new MockFileSystem();
            MockFileHashServiceFactory factory = new MockFileHashServiceFactory();
            ScannedFileStore scannedFileStore = new ScannedFileStore(fileSystem, factory);
            MockScannedFileStoreProgress progress = new MockScannedFileStoreProgress();
            factory.ScannedFiles.Add(new ScannedFile()
            {
                Id = 1,
                Hash = new byte[32],
                Path = @"C:\foo\bar.txt",
                Name = "bar.txt"
            });

            // ACT
            await scannedFileStore.RemoveFile(@"C:\foo\bar.txt", progress, 1, 1);

            // ASSERT
            Assert.AreEqual(100, progress.ReportedValues[0]);
        }
    }
}
