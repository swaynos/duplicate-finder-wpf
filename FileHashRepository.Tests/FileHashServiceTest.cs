using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
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
            var mockSet = GetMockScannedFiles(0);
            var mockContext = GetMockContext(mockSet.Object);
            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            await service.InsertScannedFileAsync(new ScannedFile()
            {
                Name = "foo",
                Path = "bar",
                Id = 1
            });

            // ASSERT
            mockSet.Verify(t => t.Add(It.IsAny<ScannedFile>()), Times.Once());
            mockContext.Verify(t => t.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task InsertScannedFileAsync_IgnoresDuplicateFile()
        {
            // ARRANGE
            var mockSet = GetMockScannedFiles(1);
            var mockContext = GetMockContext(mockSet.Object);
            var mockScannedFile = new ScannedFile()
            {
                Name = mockSet.Object.First().Name,
                Path = mockSet.Object.First().Path,
                Hash = mockSet.Object.First().Hash,
                Id = 2
            };
            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            await service.InsertScannedFileAsync(mockScannedFile);

            // ASSERT
            mockSet.Verify(t => t.Add(It.IsAny<ScannedFile>()), Times.Never());
            mockContext.Verify(t => t.SaveChangesAsync(), Times.Never());
        }

        [TestMethod]
        public async Task PurgeScannedLocations_RemovesAllScannedFileAndLocationEntities()
        {
            // ARRANGE
            var mockScannedFiles = GetMockScannedFiles(3);
            var mockScannedLocations = GetMockScannedLocations(1);
            var mockContext = GetMockContext(mockScannedFiles.Object, mockScannedLocations.Object);

            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
           await service.PurgeScannedLocationsAsync(null);

            // ASSERT
            mockScannedFiles.Verify(t => t.RemoveRange(It.IsAny<IEnumerable<ScannedFile>>()), Times.Once(), 
                    "The complete range of entities was not removed from the entity set.");
            mockScannedLocations.Verify(t => t.RemoveRange(It.IsAny<IEnumerable<ScannedLocation>>()), Times.Once(),
                    "The complete range of entities was not removed from the entity set.");
            mockContext.Verify(t => t.SaveChangesAsync(), Times.Once());
        }

        /// <summary>
        /// This automates an end to end test of PurgeScannedLocations method (including SQL). 
        /// Remove the [Ignore] attribute to run this test.
        /// </summary>
        [TestMethod]
        [Ignore] // Ignore non Unit Tests
        public async Task PurgeScannedLocations_SqlCommandPurgesCorrectScannedFiles()
        {
            // ARRANGE
            List<string> results;
            List<string> locations = new List<string>()
            {
                "C:\\Foo"
            };
            using (FileHashService service = new FileHashService(new FileHashEntities()))
            {
                await InsertScannedFileAsync(service, new byte[32], "Foobar", "C:\\Foo\\Foobar");
                await InsertScannedFileAsync(service, new byte[32], "Foobar", "C:\\Bar\\Foobar");
                await InsertScannedFileAsync(service, new byte[32], "Foobar", "C:\\Foo\\Bar\\Foobar");
                await InsertScannedFileAsync(service, new byte[32], "Foobar", "C:\\Foobar\\Foobar");
                await InsertScannedFileAsync(service, new byte[32], "Foobar", "C:\\Foobar");

                // ACT
                await service.PurgeScannedLocationsAsync(locations);
                results = await service.ListScannedFilePathsAsync(null);
                results.Sort();

                // Cleanup any remaining entities we created
                await service.PurgeScannedLocationsAsync(null);
            }

            // ASSERT
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("C:\\Bar\\Foobar", results[0]);
            Assert.AreEqual("C:\\Foobar", results[1]);
            Assert.AreEqual("C:\\Foobar\\Foobar", results[2]);

        }

        /// <summary>
        /// This automates an end to end test of PurgeScannedLocations method (including SQL). 
        /// Remove the [Ignore] attribute to run this test.
        /// </summary>
        [TestMethod]
        [Ignore] // Ignore non Unit Tests
        public async Task PurgeScannedLocations_SqlCommandRemovesScannedLocations()
        {
            // ARRANGE
            List<string> results;
            List<string> locations = new List<string>()
            {
                "C:\\Foo"
            };
            using (FileHashEntities context = new FileHashEntities())
            {
                using (FileHashService service = new FileHashService(context))
                {
                    ScannedLocation location = new ScannedLocation()
                    {
                        Path = "C:\\Foo"
                    };
                    await service.InsertScannedLocationAsync(location);
                    location = new ScannedLocation()
                    {
                        Path = "C:\\Foobar"
                    };
                    await service.InsertScannedLocationAsync(location);
                    location = new ScannedLocation()
                    {
                        Path = "C:\\Foo\\Bar"
                    };
                    await service.InsertScannedLocationAsync(location);

                    // ACT
                    await service.PurgeScannedLocationsAsync(locations);
                    results = await service.ListScannedLocationsAsync();
                    results.Sort();

                    // Cleanup any remaining entities we created
                    context.ScannedLocations.RemoveRange(context.ScannedLocations);
                    await context.SaveChangesAsync();
                }
            }

            // ASSERT
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(results[0], "C:\\Foo\\Bar");
            Assert.AreEqual(results[1], "C:\\Foobar");
        }

        [TestMethod]
        public async Task RemoveScannedFilesByFilePathAsync_NoMatches_RemovesNothing()
        {
            // ARRANGE
            string searchPath = "C:\foobar.foo";

            var mockSet = GetMockScannedFiles(3);

            var mockContext = GetMockContext(mockSet.Object);

            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            int result = await service.RemoveScannedFilesByFilePathAsync(searchPath);

            // ASSERT
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task RemoveScannedFilesByFilePathAsync_OneMatch_RemovesOne()
        {
            // ARRANGE
            string searchPath = "C:\foobar.foo";
            int removedEntities = 0;
            var mockSet = GetMockScannedFiles(3);
            UpdateMockScannedFileHash(mockSet.Object, 1, null, null, searchPath);

            // Specify what the set does when RemoveRange() is called
            mockSet.Setup(t => t.RemoveRange(It.IsAny<IEnumerable<ScannedFile>>())).Returns<IEnumerable<ScannedFile>>((entities) =>
            {
                removedEntities = entities.Count();
                return entities;
            });

            // Create our Mock Context
            var mockContext = GetMockContext(mockSet.Object);
            mockContext.Setup(t => t.SaveChangesAsync()).ReturnsAsync(() =>
            {
                return removedEntities;
            });

            // Finally create our service and provide the mock context
            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            int count = await service.RemoveScannedFilesByFilePathAsync(searchPath);

            // ASSERT
            mockContext.Verify(t => t.SaveChangesAsync(), Times.Once());
            Assert.AreEqual(1, count, "The number of items removed does not match what was expected.");
        }

        [TestMethod]
        public async Task RemoveScannedFilesByFilePathAsync_ManyMatches_RemovesMany()
        {
            // ARRANGE
            string searchPath = "C:\foobar.foo";
            int removedEntities = 0;
            var mockSet = GetMockScannedFiles(5);
            UpdateMockScannedFileHash(mockSet.Object, 1, null, null, searchPath);
            UpdateMockScannedFileHash(mockSet.Object, 2, null, null, searchPath);
            UpdateMockScannedFileHash(mockSet.Object, 3, null, null, searchPath);

            // Specify what the set does when RemoveRange() is called
            mockSet.Setup(t => t.RemoveRange(It.IsAny<IEnumerable<ScannedFile>>())).Returns<IEnumerable<ScannedFile>>((entities) =>
            {
                removedEntities = entities.Count();
                return entities;
            });

            // Create our Mock Context
            var mockContext = GetMockContext(mockSet.Object);
            mockContext.Setup(t => t.SaveChangesAsync()).ReturnsAsync(() =>
            {
                return removedEntities;
            });

            // Finally create our service and provide the mock context
            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            int count = await service.RemoveScannedFilesByFilePathAsync(searchPath);

            // ASSERT
            mockContext.Verify(t => t.SaveChangesAsync(), Times.Once());
            Assert.AreEqual(3, count, "The number of items removed does not match what was expected.");
        }

        [TestMethod]
        public async Task ReturnDuplicatesAsync_NoDuplicateHashes_ReturnsEmptyList()
        {
            // ARRANGE
            var mockSet = GetMockScannedFiles(3);
            var mockContext = GetMockContext(mockSet.Object);
            for (int i = 1; i <= 3; i++)
            {
                byte[] updatedBytes = new byte[32];
                updatedBytes[0] = System.Convert.ToByte(i);
                UpdateMockScannedFileHash(mockSet.Object, i, updatedBytes, null, null);
            }

            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            List<ScannedFile> results = await service.ReturnDuplicatesAsync();

            // ASSERT
            Assert.AreEqual(0, results.Count, "The number of found scanned files does not match what was expected");
        }

        [TestMethod]
        public async Task ReturnDuplicatesAsync_OneDuplicateHash_ReturnsTwoScannedFiles()
        {
            // ARRANGE
            var mockSet = GetMockScannedFiles(3);
            var mockContext = GetMockContext(mockSet.Object);
            // Since ultimately the intended behavior of ReturnDuplicates is to build a LINQ to Entities 
            // query, by using the same byte[] reference in our mockSet we can simulate the same behavior in the 
            // GroupBy(entity.Hash) statement of our mock set without using an IEquityComparer (which would destroy 
            // the intended behavior in the implementation). There may be other, better, ways around this limitation
            // but I stumbled upon this solution by accident.
            byte[] uniqueBytes = new byte[32];
            uniqueBytes[1] = 0x01;
            UpdateMockScannedFileHash(mockSet.Object, 1, uniqueBytes, null, null);
            UpdateMockScannedFileHash(mockSet.Object, 2, uniqueBytes, null, null);

            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            List<ScannedFile> results = await service.ReturnDuplicatesAsync();

            // ASSERT
            Assert.AreEqual(2, results.Count, "The number of found scanned files does not match what was expected");
        }

        [TestMethod]
        public async Task ReturnDuplicatesAsync_ManyDuplicateHash_ReturnsManyScannedFiles()
        {
            // ARRANGE
            var mockSet = GetMockScannedFiles(10);
            var mockContext = GetMockContext(mockSet.Object);
            // Since ultimately the intended behavior of ReturnDuplicates is to build a LINQ to Entities 
            // query, by using the same byte[] reference in our mockSet we can simulate the same behavior in the 
            // GroupBy(entity.Hash) statement of our mock set without using an IEquityComparer (which would destroy 
            // the intended behavior in the implementation). There may be other, better, ways around this limitation
            // but I stumbled upon this solution by accident.
            byte[] uniqueBytesOne = new byte[32];
            byte[] uniqueBytesTwo = new byte[32];
            uniqueBytesOne[1] = 0x01;
            uniqueBytesTwo[2] = 0x02;
            for (int i = 1; i <= 10; i++)
            {
                switch (i)
                {
                    case 1:
                    case 2:
                        UpdateMockScannedFileHash(mockSet.Object, i, uniqueBytesOne, null, null);
                        break;
                    case 3:
                    case 4:
                        UpdateMockScannedFileHash(mockSet.Object, i, uniqueBytesTwo, null, null);
                        break;
                }
            }

            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            List<ScannedFile> results = await service.ReturnDuplicatesAsync();

            // ASSERT
            Assert.AreEqual(4, results.Count, "The number of found scanned files does not match what was expected");
        }

        [TestMethod]
        public async Task ReturnDuplicatesAsync_ManyDuplicateHash_ReturnsSortedList()
        {
            // ARRANGE
            var mockSet = GetMockScannedFiles(10);
            var mockContext = GetMockContext(mockSet.Object);
            /// Please see comment in <see cref="ReturnDuplicatesAsync_ManyDuplicateHash_ReturnsManyScannedFiles"/>
            byte[] uniqueBytesOne = new byte[32];
            byte[] uniqueBytesTwo = new byte[32];
            uniqueBytesOne[1] = 0x01;
            uniqueBytesTwo[2] = 0x02;
            UpdateMockScannedFileHash(mockSet.Object, 1, uniqueBytesOne, "foo1", null);
            UpdateMockScannedFileHash(mockSet.Object, 2, uniqueBytesTwo, "bar1", null);
            UpdateMockScannedFileHash(mockSet.Object, 3, uniqueBytesTwo, "bar2", null);
            UpdateMockScannedFileHash(mockSet.Object, 4, uniqueBytesOne, "foo2", null);

            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            List<ScannedFile> results = await service.ReturnDuplicatesAsync();

            // ASSERT
            Assert.AreEqual("foo2", results[1].Name, "The collection was not sorted as expected.");
        }


        [TestMethod]
        public async Task InsertScannedLocationAsync_InsertsScannedLocation()
        {
            // ARRANGE
            var mockScannedFiles = GetMockScannedFiles(0);
            var mockScannedLocations = GetMockScannedLocations(0);
            var mockContext = GetMockContext(mockScannedFiles.Object, mockScannedLocations.Object);
            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            await service.InsertScannedLocationAsync(new ScannedLocation()
            {
                Path = "bar",
                Id = 1
            });

            // ASSERT
            mockScannedLocations.Verify(t => t.Add(It.IsAny<ScannedLocation>()), Times.Once());
            mockContext.Verify(t => t.SaveChangesAsync(), Times.Once());
        }

        [TestMethod]
        public async Task InsertScannedLocationAsync_IgnoresDuplicateScannedLocation()
        {
            // ARRANGE
            var mockScannedFiles = GetMockScannedFiles(0);
            var mockScannedLocations = GetMockScannedLocations(1, "bar");
            var mockContext = GetMockContext(mockScannedFiles.Object, mockScannedLocations.Object);
            FileHashService service = new FileHashService(mockContext.Object);

            // ACT
            await service.InsertScannedLocationAsync(new ScannedLocation()
            {
                Path = "bar",
                Id = 2
            });

            // ASSERT
            mockScannedLocations.Verify(t => t.Add(It.IsAny<ScannedLocation>()), Times.Never());
            mockContext.Verify(t => t.SaveChangesAsync(), Times.Never());
        }

        [TestMethod]
        public async Task ListScannedFilePathsAsync_ReturnsSingleFilePath()
        {
            // ARRANGE
            var mockSet = GetMockScannedFiles(1);
            var mockContext = GetMockContext(mockSet.Object);
            UpdateMockScannedFileHash(mockSet.Object, 1, null, null, "testing");
            FileHashService service = new FileHashService(mockContext.Object);

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
            var mockSet = GetMockScannedFiles(3);
            var mockContext = GetMockContext(mockSet.Object);
            FileHashService service = new FileHashService(mockContext.Object);
            UpdateMockScannedFileHash(mockSet.Object, 3, null, null, "testing");

            // ACT
            List<string> results = await service.ListScannedFilePathsAsync(null);

            // ASSERT
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("testing", results[2]);
        }

        /// <summary>
        /// This automates an end to end test of ListScannedFilePathsAsync method (including SQL). 
        /// Remove the [Ignore] attribute to run this test.
        /// </summary>
        [TestMethod]
        [Ignore] // Ignore non Unit Tests
        public async Task ListScannedFilePathsAsync_SqlQueryFiltersFiles()
        {
            // ARRANGE
            List<string> results;
            List<string> locations = new List<string>()
            {
                "C:\\Foo"
            };
            using (FileHashService service = new FileHashService(new FileHashEntities()))
            {
                await InsertScannedFileAsync(service, new byte[32], "Foobar", "C:\\Foo\\Foobar");
                await InsertScannedFileAsync(service, new byte[32], "Foobar", "C:\\Bar\\Foobar");
                await InsertScannedFileAsync(service, new byte[32], "Foobar", "C:\\Foo\\Bar\\Foobar");
                await InsertScannedFileAsync(service, new byte[32], "Foobar", "C:\\Foobar\\Foobar");
                await InsertScannedFileAsync(service, new byte[32], "Foobar", "C:\\Foobar");

                // ACT
                results = await service.ListScannedFilePathsAsync(locations);

                // Cleanup the entities we created
                await service.PurgeScannedLocationsAsync(null);
            }
            results.Sort();

            // ASSERT
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("C:\\Foo\\Bar\\Foobar", results[0]);
            Assert.AreEqual("C:\\Foo\\Foobar", results[1]);
        }

        [TestMethod]
        public void Dispose_CallsDisposeOnContext()
        {
            // ARRANGE
            FileHashEntities context = new FileHashEntities();
            FileHashService service = new FileHashService(context);

            // ACT
            service.Dispose();

            // ASSERT
            Assert.IsTrue(IsContextDisposed(context), "The FileHashEntities context is not disposed.");
        }

        /// <summary>
        /// Helper method to return a new Mock of ScannedFiles
        /// </summary>
        /// <param name="count">The number of items in the (implemented) collection</param>
        /// <returns>A Mock wrapper of a DbSet of ScannedFiles with repeated setup</returns>
        private Mock<DbSet<ScannedFile>> GetMockScannedFiles(int count, string name = "foo", string path="bar")
        {
            List<ScannedFile> data = new List<ScannedFile>();
            for(int i = 1; i <= count; i++)
            {
                data.Add(new ScannedFile
                {
                    Id = i,
                    Name = name,
                    Path = path,
                    Hash = new byte[32]
                });
            }
            IQueryable<ScannedFile> dataAsQueryable = data.AsQueryable();

            // Create our Mock DbSet
            Mock<DbSet<ScannedFile>> mockSet = new Mock<DbSet<ScannedFile>>();
            
            // The following should allow our mock set to be queryable from the mock context
            mockSet.As<IDbAsyncEnumerable<ScannedFile>>()
                    .Setup(t => t.GetAsyncEnumerator())
                    .Returns(new MockDbAsyncEnumerator<ScannedFile>(data.GetEnumerator()));

            mockSet.As<IQueryable<ScannedFile>>()
                    .Setup(t => t.Provider)
                    .Returns(new MockDbAsyncQueryProvider<ScannedFile>(dataAsQueryable.Provider));

            mockSet.As<IQueryable<ScannedFile>>().Setup(t => t.Expression).Returns(dataAsQueryable.Expression);
            mockSet.As<IQueryable<ScannedFile>>().Setup(t => t.ElementType).Returns(dataAsQueryable.ElementType);
            mockSet.As<IQueryable<ScannedFile>>().Setup(t => t.GetEnumerator()).Returns(dataAsQueryable.GetEnumerator());

            return mockSet;
        }

        /// <summary>
        /// Helper method to return a new Mock of ScannedLocations
        /// </summary>
        /// <param name="count">The number of items in the (implemented) collection</param>
        /// <returns>A Mock wrapper of the DbSet of ScannedLocations with repeated setup</returns>
        private Mock<DbSet<ScannedLocation>> GetMockScannedLocations(int count, string path="bar")
        {
            List<ScannedLocation> data = new List<ScannedLocation>();
            for (int i = 1; i <= count; i++)
            {
                data.Add(new ScannedLocation
                {
                    Id = i,
                    Path = path
                });
            }
            IQueryable<ScannedLocation> dataAsQueryable = data.AsQueryable();

            // Create our Mock DbSet
            Mock<DbSet<ScannedLocation>> mockSet = new Mock<DbSet<ScannedLocation>>();

            // The following should allow our mock set to be queryable from the mock context
            mockSet.As<IDbAsyncEnumerable<ScannedLocation>>()
                    .Setup(t => t.GetAsyncEnumerator())
                    .Returns(new MockDbAsyncEnumerator<ScannedLocation>(data.GetEnumerator()));

            mockSet.As<IQueryable<ScannedLocation>>()
                    .Setup(t => t.Provider)
                    .Returns(new MockDbAsyncQueryProvider<ScannedLocation>(dataAsQueryable.Provider));

            mockSet.As<IQueryable<ScannedLocation>>().Setup(t => t.Expression).Returns(dataAsQueryable.Expression);
            mockSet.As<IQueryable<ScannedLocation>>().Setup(t => t.ElementType).Returns(dataAsQueryable.ElementType);
            mockSet.As<IQueryable<ScannedLocation>>().Setup(t => t.GetEnumerator()).Returns(dataAsQueryable.GetEnumerator());

            return mockSet;
        }

        /// <summary>
        /// Helper method to return a new Mock of FileHashEntities
        /// </summary>
        /// <param name="scannedFiles">The ScannedFile dbSet to construct the context with </param>
        /// <returns>A new FileHashEntities dbContext</returns>
        private Mock<FileHashEntities> GetMockContext(DbSet<ScannedFile> scannedFiles) 
        {
            return GetMockContext(scannedFiles, null);
        }

        /// <summary>
        /// Helper method to return a new Mock of FileHashEntities
        /// </summary>
        /// <param name="scannedFiles">The ScannedFile dbSet to construct the context with </param>
        /// <param name="scannedLocations">The ScannedLocation dbSet to construct the context with</param>
        /// <returns>A new FileHashEntities dbContext</returns>
        private Mock<FileHashEntities> GetMockContext(DbSet<ScannedFile> scannedFiles, DbSet<ScannedLocation> scannedLocations)
        {
            Mock<FileHashEntities> mockContext = new Mock<FileHashEntities>();
            mockContext.Setup(t => t.ScannedFiles).Returns(scannedFiles);
            mockContext.Setup(t => t.ScannedLocations).Returns(scannedLocations);
            return mockContext;
        }

        /// <summary>
        /// Helper method to update the contents of a ScannedFile Hash by Id
        /// </summary>
        /// <param name="dbSet">The DbSet of ScannedFiles to query the id against</param>
        /// <param name="id">The Id of the ScannedFile as defined in the dbSet to update the hash</param>
        /// <param name="data">The byte array will replace the Hash on the ScannedFile</param>
        /// <param name="name">The file name that will replace the Name on the ScannedFile</param>
        /// <param name="path">The path that will replace the Path on the ScannedFile</param>
        private void UpdateMockScannedFileHash(DbSet<ScannedFile> dbSet, int id, byte[] data, string name, string path)
        {
            ScannedFile file = dbSet.Single(t => t.Id.Equals(id));
            if (data != null)
            {
                file.Hash = data;
            }
            if (name != null)
            {
                file.Name = name;
            }
            if (path != null)
            {
                file.Path = path;
            }
        }

        /// <summary>
        /// Helper method to simplify inserting new ScannedFile entities to the FileHashService
        /// </summary>
        /// <param name="service">The FileHashService instance to call InsertScannedFileAsync() on</param>
        /// <param name="hash">A length 32 byte[] that contains the file hash</param>
        /// <param name="name">The name of the file</param>
        /// <param name="path">The file path of the file</param>
        private async Task InsertScannedFileAsync(FileHashService service, byte[] hash, string name, string path)
        {
            ScannedFile file = new ScannedFile()
            {
                Hash = hash,
                Name = name,
                Path = path
            };
            await service.InsertScannedFileAsync(file);
        }

        /// <summary>
        /// Use reflection to determine if the provided DbContext is already disposed
        /// </summary>
        /// <param name="context">The DbContext to test</param>
        private bool IsContextDisposed(DbContext context)
        {
            // https://pholpar.wordpress.com/2017/11/29/how-to-detect-if-your-dbconext-is-already-disposed/
            bool result = true;

            Type typeDbContext = typeof(DbContext);
            Type typeInternalContext = typeDbContext.Assembly.GetType("System.Data.Entity.Internal.InternalContext");

            var fi_InternalContext = typeDbContext.GetField("_internalContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pi_IsDisposed = typeInternalContext.GetProperty("IsDisposed");

            var ic = fi_InternalContext.GetValue(context);

            if (ic != null)
            {
                result = (bool)pi_IsDisposed.GetValue(ic);
            }

            return result;
        }
    }
}
