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
            Mock<ProgressBar> control = new Mock<ProgressBar>();
            ScanningProgress scanningProgress = new ScanningProgress(control.Object);

            // ACT
            scanningProgress.Report(69);

            // ASSERT
            Assert.AreEqual(69, control.Object.Value);
        }

        [TestMethod]
        public void Report_ValueGreaterThan100Percent_Is100Percent()
        {
            // ARRANGE
            Mock<ProgressBar> control = new Mock<ProgressBar>();
            ScanningProgress scanningProgress = new ScanningProgress(control.Object);

            // ACT
            scanningProgress.Report(9001);

            // ASSERT
            Assert.AreEqual(100, control.Object.Value);
        }

        [TestMethod]
        public void Report_DivisorReducesValue()
        {
            // ARRANGE
            Mock<ProgressBar> control = new Mock<ProgressBar>();
            ScanningProgress scanningProgress = new ScanningProgress(control.Object, 0, 2);

            // ACT
            scanningProgress.Report(100);

            // ASSERT
            Assert.AreEqual(50, control.Object.Value);
        }

        [TestMethod]
        public void Report_AddendAddedToValue()
        {
            // ARRANGE
            Mock<ProgressBar> control = new Mock<ProgressBar>();
            ScanningProgress scanningProgress = new ScanningProgress(control.Object, 50, 1);

            // ACT
            scanningProgress.Report(1);

            // ASSERT
            Assert.AreEqual(51, control.Object.Value);
        }

        [TestMethod]
        public void Report_AddendAddedToDivision()
        {
            // ARRANGE
            Mock<ProgressBar> control = new Mock<ProgressBar>();
            ScanningProgress scanningProgress = new ScanningProgress(control.Object, 50, 2);

            // ACT
            scanningProgress.Report(100);

            // ASSERT
            Assert.AreEqual(100, control.Object.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void Report_DivisorIsZero_ThrowsException()
        {
            // ARRANGE
            Mock<ProgressBar> control = new Mock<ProgressBar>();
            ScanningProgress scanningProgress = new ScanningProgress(control.Object, 0, 0);

            // ACT
            scanningProgress.Report(1);

            // ASSERT (handled with ExpectedException)
        }
    }
}
