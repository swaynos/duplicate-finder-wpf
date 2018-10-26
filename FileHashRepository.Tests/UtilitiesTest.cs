using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using FileHashRepository.Utilities;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;

namespace FileHashRepository.Tests
{
    [TestClass]
    public class UtilitiesTest
    {

        [TestMethod]
        public async Task FileHash_ComputeFileHashAsync_ReturnsHash()
        {
            // ARRANGE
            MockFileData mockFileData = new MockFileData("This is a test file.");
            Dictionary<string, MockFileData> dictionaryMockFileData = new Dictionary<string, MockFileData>();

            string key = "foobar";
            dictionaryMockFileData.Add(key, mockFileData);
            MockFileSystem fileSystem = new MockFileSystem(dictionaryMockFileData);

            FileHash filehash = new FileHash(fileSystem);

            // ACT
            byte[] result = await filehash.ComputeFileHashAsync(key);

            // ASSERT
            Assert.AreEqual(result.Length, 32, "The result did not match what was expected.");
            Assert.IsTrue(result[0]> 0, "The result did not match what was expected.");
        }

        [TestMethod]
        public void SqlQuery_FormatSqlQuery_RootDrive_ReturnsCorrectQuery()
        {
            // ARRANGE
            string query = "SELECT * FROM [Foo] WHERE [Path] = {0}\\";
            string locationPath = "C:\\";

            // ACT
            string result = SqlQuery.FormatSqlQuery(query, locationPath);

            // ASSERT
            string expectedResult = "SELECT * FROM [Foo] WHERE [Path] = C:\\";
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void SqlQuery_FormatSqlQuery_Directory_ReturnsCorrectQuery()
        {
            // ARRANGE
            string query = "SELECT * FROM [Foo] WHERE [Path] = {0}\\";
            string locationPath = "C:\\foo\\bar";

            // ACT
            string result = SqlQuery.FormatSqlQuery(query, locationPath);

            // ASSERT
            string expectedResult = "SELECT * FROM [Foo] WHERE [Path] = C:\\foo\\bar\\";
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void SqlQuery_FormatSqlQuery_DirectoryTrailingBackslash_ReturnsCorrectQuery()
        {
            // ARRANGE
            string query = "SELECT * FROM [Foo] WHERE [Path] = {0}\\";
            string locationPath = "C:\\foo\\bar\\";

            // ACT
            string result = SqlQuery.FormatSqlQuery(query, locationPath);

            // ASSERT
            string expectedResult = "SELECT * FROM [Foo] WHERE [Path] = C:\\foo\\bar\\";
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void SqlQuery_FormatSqlQuery_MultipleBackslashes_RemovesAllBackslashes()
        {
            // ARRANGE
            string query = "{0}";
            string locationPath = "foo\\\\\\";

            // ACT
            string result = SqlQuery.FormatSqlQuery(query, locationPath);

            // ASSERT
            string expectedResult = "foo";
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SqlQuery_FormatSqlQuery_InvalidLocationPath_Short_ThrowsException()
        {
            // ARRANGE
            string query = "{0}";
            string locationPath = "\\\\";

            // ACT
            string result = SqlQuery.FormatSqlQuery(query, locationPath);

            // ASSERT (handled with [ExpectedException] attribute)
        }
    }
}
