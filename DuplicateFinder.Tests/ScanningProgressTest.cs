using System;
using System.Windows.Controls;
using DuplicateFinder.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DuplicateFinder.Tests
{
    [TestClass]
    public class ScanningProgressTest
    {
        [TestMethod]
        public void Report_UpdatesControlValue()
        {
            // ARRANGE
            int reported = 0;
            Action<double> action = (d) =>
            {
                reported = (int) d;
            };
            ScanningProgress scanningProgress = new ScanningProgress(action);

            // ACT
            scanningProgress.Report(69);

            // ASSERT
            Assert.AreEqual(69, reported);
        }

        [TestMethod]
        public void Report_ValueGreaterThan100Percent_Is100Percent()
        {
            // ARRANGE
            int reported = 0;
            Action<double> action = (d) =>
            {
                reported = (int)d;
            };
            ScanningProgress scanningProgress = new ScanningProgress(action);

            // ACT
            scanningProgress.Report(9001);

            // ASSERT
            Assert.AreEqual(100, reported);
        }

        [TestMethod]
        public void Report_DivisorReducesValue()
        {
            // ARRANGE
            int reported = 0;
            Action<double> action = (d) =>
            {
                reported = (int)d;
            };
            ScanningProgress scanningProgress = new ScanningProgress(action, 0, 2);

            // ACT
            scanningProgress.Report(100);

            // ASSERT
            Assert.AreEqual(50, reported);
        }

        [TestMethod]
        public void Report_AddendAddedToValue()
        {
            // ARRANGE
            int reported = 0;
            Action<double> action = (d) =>
            {
                reported = (int)d;
            };
            ScanningProgress scanningProgress = new ScanningProgress(action, 50, 1);

            // ACT
            scanningProgress.Report(1);

            // ASSERT
            Assert.AreEqual(51, reported);
        }

        [TestMethod]
        public void Report_AddendAddedToDivision()
        {
            // ARRANGE
            int reported = 0;
            Action<double> action = (d) =>
            {
                reported = (int)d;
            };
            ScanningProgress scanningProgress = new ScanningProgress(action, 50, 2);

            // ACT
            scanningProgress.Report(100);

            // ASSERT
            Assert.AreEqual(100, reported);
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void Report_DivisorIsZero_ThrowsException()
        {
            // ARRANGE
            int reported = 0;
            Action<double> action = (d) =>
            {
                reported = (int)d;
            };
            ScanningProgress scanningProgress = new ScanningProgress(action, 0, 0);

            // ACT
            scanningProgress.Report(1);

            // ASSERT (handled with ExpectedException)
        }
    }
}
