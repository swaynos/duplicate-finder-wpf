using System;
using DuplicateFinder.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DuplicateFinder.Tests
{
    [TestClass]
    public class ScannedFileHashComparerTest
    {
        [TestMethod]
        public void GetHashCode_ReturnsUniqueHash()
        {
            // ARRANGE
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();
            byte[] fileHashOne = new byte[32];
            byte[] fileHashTwo = new byte[32];
            fileHashOne[0] = 0x01;
            fileHashOne[1] = 0x02;
            fileHashOne[2] = 0x03;
            fileHashOne[3] = 0x04;
            fileHashTwo[0] = 0x04;
            fileHashTwo[1] = 0x03;
            fileHashTwo[2] = 0x02;
            fileHashTwo[3] = 0x01;

            // ACT
            int hashOne = comparer.GetHashCode(fileHashOne);
            int hashTwo = comparer.GetHashCode(fileHashTwo);

            // ASSERT
            Assert.AreNotEqual(hashOne, hashTwo, "The two hash codes were equal when expected to be different");
        }

        [TestMethod]
        public void GetHashCode_ArrayLessThanFourBytes_ReturnsInt()
        {
            // ARRANGE
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();
            byte[] fileHash = new byte[2];
            fileHash[0] = 0x01;
            fileHash[1] = 0x02;

            // ACT
            int hashOne = comparer.GetHashCode(fileHash);

            // ASSERT (if no exception the test passes)
        }


        [TestMethod]
        public void Equals_BothNull_IsEqual()
        {
            // ARRANGE
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();

            // ACT
            bool result = comparer.Equals(null, null);

            // ASSERT
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_OnlyOneNull_IsNotEqual()
        {
            // ARRANGE
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();
            byte[] fileHashOne = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                fileHashOne[i] = 0xFF;
            }

            // ACT
            bool result = comparer.Equals(fileHashOne, null);

            // ASSERT
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_SameReference_IsEqual()
        {
            // ARRANGE
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();
            byte[] fileHashOne = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                fileHashOne[i] = 0xFF;
            }

            // ACT
            bool result = comparer.Equals(fileHashOne, fileHashOne);

            // ASSERT
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_DifferentLength_IsNotEqual()
        {
            // ARRANGE
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();
            byte[] fileHashOne = new byte[32];
            byte[] fileHashTwo = new byte[16];

            // ACT
            bool result = comparer.Equals(fileHashOne, fileHashTwo);

            // ASSERT
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_EqualContent_IsEqual()
        {
            // ARRANGE
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();
            byte[] fileHashOne = new byte[32];
            byte[] fileHashTwo = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                fileHashOne[i] = 0xFF;
                fileHashTwo[i] = 0xFF;
            }

            // ACT
            bool result = comparer.Equals(fileHashOne, fileHashTwo);

            // ASSERT
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_OnlyFirstFourBytesEqual_IsNotEqual()
        {
            // ARRANGE
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();
            byte[] fileHashOne = new byte[32];
            byte[] fileHashTwo = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                fileHashOne[i] = 0xFF;
                fileHashTwo[i] = 0xFE;
            }
            fileHashOne[0] = 0x01;
            fileHashOne[1] = 0x02;
            fileHashOne[2] = 0x03;
            fileHashOne[3] = 0x04;
            fileHashTwo[0] = 0x01;
            fileHashTwo[1] = 0x02;
            fileHashTwo[2] = 0x03;
            fileHashTwo[3] = 0x04;

            // ACT
            bool result = comparer.Equals(fileHashOne, fileHashTwo);

            // ASSERT
            Assert.IsFalse(result);
        }
    }
}
