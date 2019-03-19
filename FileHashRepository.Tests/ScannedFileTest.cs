using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileHashRepository.Tests
{
    [TestClass]
    public class ScannedFileTest
    {
        [TestMethod]
        public void Equals_EqualObjects_ReturnTrue()
        {
            // ARRANGE
            ScannedFile one = new ScannedFile()
            {
                Name = "foo",
                Path = "bar",
                Hash = new byte[32]
            };
            ScannedFile two = new ScannedFile()
            {
                Name = "foo",
                Path = "bar",
                Hash = new byte[32]
            };
            one.Hash[0] = 0x99;
            two.Hash[0] = 0x99;

            // ACT
            bool result = one.Equals(two);

            // ASSERT
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_UnEqualHash_ReturnFalse()
        {
            // ARRANGE
            ScannedFile one = new ScannedFile()
            {
                Name = "foo",
                Path = "bar",
                Hash = new byte[32]
            };
            ScannedFile two = new ScannedFile()
            {
                Name = "foo",
                Path = "bar",
                Hash = new byte[32]
            };
            one.Hash[0] = 0x99;
            two.Hash[0] = 0x11;

            // ACT
            bool result = one.Equals(two);

            // ASSERT
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_UnEqualName_ReturnFalse()
        {
            // ARRANGE
            ScannedFile one = new ScannedFile()
            {
                Name = "foo",
                Path = "bar",
                Hash = new byte[32]
            };
            ScannedFile two = new ScannedFile()
            {
                Name = "foo2",
                Path = "bar",
                Hash = new byte[32]
            };
            one.Hash[0] = 0x99;
            two.Hash[0] = 0x99;

            // ACT
            bool result = one.Equals(two);

            // ASSERT
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_UnEqualPath_ReturnFalse()
        {
            // ARRANGE
            ScannedFile one = new ScannedFile()
            {
                Name = "foo",
                Path = "bar",
                Hash = new byte[32]
            };
            ScannedFile two = new ScannedFile()
            {
                Name = "foo",
                Path = "bar2",
                Hash = new byte[32]
            };
            one.Hash[0] = 0x99;
            two.Hash[0] = 0x99;

            // ACT
            bool result = one.Equals(two);

            // ASSERT
            Assert.IsFalse(result);
        }
    }
}
