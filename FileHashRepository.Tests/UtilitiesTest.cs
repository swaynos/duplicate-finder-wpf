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
    }
}
