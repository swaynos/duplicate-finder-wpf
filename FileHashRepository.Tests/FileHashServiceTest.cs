using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileHashRepository.Tests.Mocks;

namespace FileHashRepository.Tests
{
    [TestClass]
    public class FileHashServiceTest
    {
        [TestMethod]
        public async Task InsertScannedFileAsync_InsertsScannedFile()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData()).Returns(() => new List<ScannedFile>().AsQueryable());

            // ACT
            await service.InsertScannedFileAsync(new ScannedFile()
            {
                Name = "foo",
                Path = "bar"
            });

            // ASSERT
            files.Verify(t => t.InsertData(It.IsAny<ScannedFile>()), Times.Once());
        }

        [TestMethod]
        public async Task InsertScannedFileAsync_IgnoresDuplicateFile()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            var mockScannedFile = new ScannedFile()
            {
                Name = "foo",
                Path = "bar",
                Hash = new byte[32]
            };
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData()).Returns(() => new List<ScannedFile>()
            {
                new ScannedFile()
                {
                    Name = "foo",
                    Path = "bar",
                    Hash =  new byte[32]
                }
            }.AsQueryable());

            // ACT
            await service.InsertScannedFileAsync(mockScannedFile);

            // ASSERT
            files.Verify(t => t.InsertData(It.IsAny<ScannedFile>()), Times.Never());
        }

        [TestMethod]
        public async Task PurgeScannedLocations_RemovesAllScannedFileAndLocationEntities()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);

            // ACT
           await service.PurgeScannedLocationsAsync(null);

            // ASSERT
            files.Verify(t => t.PurgeData(It.IsAny<IQueryable<ScannedFile>>()), Times.Once(), 
                    "The complete range of entities was not removed from the entity set.");
            locations.Verify(t => t.PurgeData(It.IsAny<IQueryable<ScannedLocation>>()), Times.Once(),
                    "The complete range of entities was not removed from the entity set.");
        }

        /// <summary>
        /// This automates an end to end test of PurgeScannedLocations method. 
        /// </summary>
        [TestMethod]
        public async Task PurgeScannedLocations_PurgesCorrectScannedFiles()
        {
            // ARRANGE
            List<ScannedLocation> locationData = new List<ScannedLocation>()
            {
                new ScannedLocation()
                {
                    Path = "C:\\Foo"
                }
            };
            List<ScannedFile> fileData = new List<ScannedFile>()
            {
                new ScannedFile()
                {
                    Name = "Foobar",
                    Path = "C:\\Foo\\Foobar",
                    Hash = new byte[32]
                },
                new ScannedFile()
                {
                    Name = "Foobar",
                    Path = "C:\\Bar\\Foobar",
                    Hash = new byte[32]
                },
                new ScannedFile()
                {
                    Name = "Foobar",
                    Path = "C:\\Foo\\Bar\\Foobar",
                    Hash = new byte[32]
                },
                new ScannedFile()
                {
                    Name = "Foobar",
                    Path = "C:\\Foobar\\Foobar",
                    Hash = new byte[32]
                },
                new ScannedFile()
                {
                    Name = "Foobar",
                    Path = "C:\\Foobar",
                    Hash = new byte[32]
                }
            };
            var files = new DataCache<ScannedFile>(fileData);
            var locations = new DataCache<ScannedLocation>(locationData);
            FileHashService service = new FileHashService(files, locations);


            // ACT
            await service.PurgeScannedLocationsAsync(locationData.Select(t => t.Path).ToList());

            // ASSERT
            List<string> results = await service.ListScannedFilePathsAsync(null);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("C:\\Bar\\Foobar", results[0]);
            Assert.AreEqual("C:\\Foobar\\Foobar", results[1]);
            Assert.AreEqual("C:\\Foobar", results[2]);
           

        }

        /// <summary>
        /// This automates an end to end test of PurgeScannedLocations method.
        /// </summary>
        [TestMethod]
        public async Task PurgeScannedLocations_RemovesScannedLocations()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData()).Returns(() =>
            {
                return new List<ScannedFile>()
                {
                    new ScannedFile()
                    {
                        Name = "Foobar",
                        Path = "C:\\Foo",
                        Hash = new byte[32]
                    },
                    new ScannedFile()
                    {
                        Name = "Foobar",
                        Path = "C:\\Foobar",
                        Hash = new byte[32]
                    },
                    new ScannedFile()
                    {
                        Name = "Foobar",
                        Path = "C:\\Foo\\Bar",
                        Hash = new byte[32]
                    }
                }.AsQueryable();
            });
            locations.Setup(t => t.ListData()).Returns(() =>
            {
                return new List<ScannedLocation>()
                {
                    new ScannedLocation()
                    {
                        Path = "C:\\Foo"
                    },
                    new ScannedLocation()
                    {
                        Path = "C:\\Foobar"
                    },
                    new ScannedLocation()
                    {
                        Path = "C:\\Foo\\Bar"
                    }
                }.AsQueryable();
            });

            // ACT
            await service.PurgeScannedLocationsAsync(new List<string>()
            {
                "C:\\Foo"
            });

            // ASSERT
            locations.Verify(t => t.PurgeData(It.IsAny<IQueryable<ScannedLocation>>()), Times.Once);
            // ToDo: Assert right collection of ScannedLocations
        }

        [TestMethod]
        public async Task RemoveScannedFilesByFilePathAsync_NoMatches_RemovesNothing()
        {
            // ARRANGE
            string searchPath = "C:\foobar.foo";
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);

            // ACT
            int result = await service.RemoveScannedFilesByFilePathAsync(searchPath);

            // ASSERT
            // ToDo: Fix this test, this will likely be a false positive
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task RemoveScannedFilesByFilePathAsync_OneMatch_RemovesOne()
        {
            // ARRANGE
            string searchPath = "C:\\foobar.foo";
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData()).Returns(() =>
            {
                return new List<ScannedFile>()
                {
                    new ScannedFile()
                    {
                        Name = "foo",
                        Path = "C:\\foobar.foo"
                    }
                }.AsQueryable();
            });

            // ACT
            int result = await service.RemoveScannedFilesByFilePathAsync(searchPath);

            // ASSERT
            Assert.AreEqual(1, result, "The number of items removed does not match what was expected.");
        }

        [TestMethod]
        public async Task RemoveScannedFilesByFilePathAsync_ManyMatches_RemovesMany()
        {
            // ARRANGE
            string searchPath = "C:\\foobar.foo";
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData()).Returns(() =>
            {
                return new List<ScannedFile>()
                {
                    new ScannedFile()
                    {
                        Name = "foobar.foo",
                        Path = "C:\\foobar.foo"
                    },
                    new ScannedFile()
                    {
                        Name = "foobar.foo",
                        Path = "C:\\foobar.foo"
                    },
                    new ScannedFile()
                    {
                        Name = "foobar.foo",
                        Path = "C:\\foo\\foobar.foo"
                    }
                }.AsQueryable();
            });

            // ACT
            int result = await service.RemoveScannedFilesByFilePathAsync(searchPath);

            // ASSERT
            Assert.AreEqual(2, result, "The number of items removed does not match what was expected.");
        }

        [TestMethod]
        public async Task ReturnDuplicatesAsync_NoDuplicateHashes_ReturnsEmptyList()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData())
                .Returns(() =>
                {
                    List<ScannedFile> scannedFiles = new List<ScannedFile>();
                    for (int i = 1; i <= 3; i++)
                    {
                        byte[] updatedBytes = new byte[32];
                        updatedBytes[0] = System.Convert.ToByte(i);
                        scannedFiles.Add(new ScannedFile()
                        {
                            Hash = updatedBytes
                        });
                    }
                    return scannedFiles.AsQueryable();
                });

            // ACT
            List<ScannedFile> results = await service.ReturnDuplicatesAsync();

            // ASSERT
            Assert.AreEqual(0, results.Count, "The number of found scanned files does not match what was expected");
        }

        [TestMethod]
        public async Task ReturnDuplicatesAsync_OneDuplicateHash_ReturnsTwoScannedFiles()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData())
                .Returns(() =>
                {
                    byte[] uniqueBytesOne = new byte[32];
                    byte[] uniqueBytesTwo = new byte[32];
                    List<ScannedFile> scannedFiles = new List<ScannedFile>()
                    {
                        new ScannedFile()
                        {
                            Hash = new byte[32]
                        },
                        new ScannedFile()
                        {
                            Hash = new byte[32]
                        },
                        new ScannedFile()
                        {
                            Hash = new byte[32]
                        }
                    };
                    uniqueBytesOne[0] = 0x11;
                    uniqueBytesTwo[0] = 0xFF;
                    Array.Copy(uniqueBytesOne, scannedFiles[0].Hash, 32);
                    Array.Copy(uniqueBytesOne, scannedFiles[1].Hash, 32);
                    Array.Copy(uniqueBytesTwo, scannedFiles[2].Hash, 32);
                    return scannedFiles.AsQueryable();
                });

            // ACT
            List<ScannedFile> results = await service.ReturnDuplicatesAsync();

            // ASSERT
            Assert.AreEqual(2, results.Count, "The number of found scanned files does not match what was expected");
        }

        [TestMethod]
        public async Task ReturnDuplicatesAsync_ManyDuplicateHash_ReturnsManyScannedFiles()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData())
                .Returns(() =>
                {
                    byte[] uniqueBytesOne = new byte[32];
                    byte[] uniqueBytesTwo = new byte[32];
                    uniqueBytesOne[1] = 0x01;
                    uniqueBytesTwo[2] = 0x02;
                    List<ScannedFile> scannedFiles = new List<ScannedFile>();
                    for (int i = 1; i <= 10; i++)
                    {
                        ScannedFile file = new ScannedFile()
                        {
                            Hash = new byte[32]
                        };
                        switch (i)
                        {
                            case 1:
                            case 2:
                                Array.Copy(uniqueBytesOne, file.Hash, 32);
                                break;
                            case 3:
                            case 4:
                                Array.Copy(uniqueBytesTwo, file.Hash, 32);
                                break;
                            default:
                                file.Hash[31] = (byte)i;
                                break;

                        }
                        scannedFiles.Add(file);
                    }
                    return scannedFiles.AsQueryable();
                });

            // ACT
            List<ScannedFile> results = await service.ReturnDuplicatesAsync();

            // ASSERT
            Assert.AreEqual(4, results.Count, "The number of found scanned files does not match what was expected");
        }

        [TestMethod]
        public async Task ReturnDuplicatesAsync_ManyDuplicateHash_ReturnsSortedList()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData())
                .Returns(() =>
                {
                    byte[] uniqueBytesOne = new byte[32];
                    byte[] uniqueBytesTwo = new byte[32];
                    uniqueBytesOne[1] = 0x01;
                    uniqueBytesTwo[2] = 0x02;
                    List<ScannedFile> scannedFiles = new List<ScannedFile>();
                    for (int i = 1; i <= 10; i++)
                    {
                        ScannedFile file = new ScannedFile()
                        {
                            Hash = new byte[32]
                        };
                        switch (i)
                        {
                            case 1:
                                file.Name = "foo1";
                                Array.Copy(uniqueBytesOne, file.Hash, 32);
                                break;
                            case 2:
                                file.Name = "bar1";
                                Array.Copy(uniqueBytesTwo, file.Hash, 32);
                                break;
                            case 3:
                                file.Name = "bar2";
                                Array.Copy(uniqueBytesTwo, file.Hash, 32);
                                break;
                            case 4:
                                file.Name = "foo2";
                                Array.Copy(uniqueBytesOne, file.Hash, 32);
                                break;
                        }
                        scannedFiles.Add(file);
                    }
                    return scannedFiles.AsQueryable();
                });

            // ACT
            List<ScannedFile> results = await service.ReturnDuplicatesAsync();

            // ASSERT
            Assert.AreEqual("foo2", results[1].Name, "The collection was not sorted as expected.");
        }

        [TestMethod]
        public async Task InsertScannedLocationAsync_InsertsScannedLocation()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);

            // ACT
            await service.InsertScannedLocationAsync(new ScannedLocation()
            {
                Path = "bar"
            });

            // ASSERT
            locations.Verify(t => t.InsertData(It.IsAny<ScannedLocation>()), Times.Once());
        }

        [TestMethod]
        public async Task InsertScannedLocationAsync_IgnoresDuplicateScannedLocation()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            locations.Setup(t => t.ListData())
                .Returns(() =>
                {
                    List<ScannedLocation> scannedLocations = new List<ScannedLocation>()
                    {
                        new ScannedLocation()
                        {
                            Path = "bar"
                        }
                    };
                    return scannedLocations.AsQueryable();
                });

            // ACT
            await service.InsertScannedLocationAsync(new ScannedLocation()
            {
                Path = "bar"
            });

            // ASSERT
            locations.Verify(t => t.InsertData(It.IsAny<ScannedLocation>()), Times.Never());
        }

        [TestMethod]
        public async Task ListScannedFilePathsAsync_ReturnsSingleFilePath()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData())
                .Returns(() =>
                {
                    List<ScannedFile> scannedFiles = new List<ScannedFile>()
                    {
                        new ScannedFile()
                        {
                            Path = "testing"
                        }
                    };
                    return scannedFiles.AsQueryable();
                });

            // ACT
            List<string> results = await service.ListScannedFilePathsAsync(null);

            // ASSERT
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("testing", results[0]);
        }

        [TestMethod]
        public async Task ListScannedFilePathsAsync_ReturnsManyFilePaths()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData())
                .Returns(() =>
                {
                    List<ScannedFile> scannedFiles = new List<ScannedFile>();
                    for (int i = 0; i < 3; i++)
                    {
                        scannedFiles.Add(new ScannedFile()
                        {
                            Path = "testing"
                        });
                    }
                    return scannedFiles.AsQueryable();
                });

            // ACT
            List<string> results = await service.ListScannedFilePathsAsync(null);

            // ASSERT
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("testing", results[2]);
        }

        /// <summary>
        /// This automates an end to end test of ListScannedFilePathsAsync method.
        /// </summary>
        [TestMethod]
        public async Task ListScannedFilePathsAsync_FiltersFiles()
        {
            // ARRANGE
            var files = new Mock<IDataCache<ScannedFile>>();
            var locations = new Mock<IDataCache<ScannedLocation>>();
            FileHashService service = new FileHashService(files.Object, locations.Object);
            files.Setup(t => t.ListData())
                .Returns(() =>
                {
                    List<ScannedFile> scannedFiles = new List<ScannedFile>()
                    {
                        new ScannedFile()
                        {
                            Name = "Foobar",
                            Path = "C:\\Foo\\Foobar"
                        },
                        new ScannedFile()
                        {
                            Name = "Foobar",
                            Path = "C:\\Bar\\Foobar"
                        },
                        new ScannedFile()
                        {
                            Name = "Foobar",
                            Path = "C:\\Foo\\Bar\\Foobar"
                        },
                        new ScannedFile()
                        {
                            Name = "Foobar",
                            Path = "C:\\Foobar\\Foobar"
                        },
                        new ScannedFile()
                        {
                            Name = "Foobar",
                            Path = "C:\\Foobar"
                        }
                    };
                    return scannedFiles.AsQueryable();
                });

            // ACT
            var results = await service.ListScannedFilePathsAsync(new List<string>()
            {
                "C:\\Foo"
            });


            // ASSERT
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(
                results[0].Equals("C:\\Foo\\Foobar", StringComparison.InvariantCultureIgnoreCase), 
                "The wrong path was returned.");
            Assert.IsTrue(
                results[1].Equals("C:\\Foo\\Bar\\Foobar", StringComparison.InvariantCultureIgnoreCase), 
                "The wrong path was returned.");
        }
    }
}
