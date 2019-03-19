using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileHashRepository.Tests
{
    [TestClass]
    public class ScannedLocationTest
    {
        [TestMethod]
        public void Equals_EqualObjects_ReturnTrue()
        {
            // ARRANGE
            ScannedLocation one = new ScannedLocation()
            {
                Path = "foo"
            };
            ScannedLocation two = new ScannedLocation()
            {
                Path = "foo"
            };

            // ACT
            bool result = one.Equals(two);

            // ASSERT
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_UnEqualPath_ReturnFalse()
        {
            // ARRANGE
            ScannedLocation one = new ScannedLocation()
            {
                Path = "foo"
            };
            ScannedLocation two = new ScannedLocation()
            {
                Path = "bar"
            };

            // ACT
            bool result = one.Equals(two);

            // ASSERT
            Assert.IsFalse(result);
        }
    }
}
