using System;
using System.Windows.Forms;
using DuplicateFinder.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DuplicateFinder.Tests
{
    [TestClass]
    public class FolderBrowserDialogWrapperTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetPathFromDialog_InvalidDialogType_ThrowsException()
        {
            // ARRANGE
            FolderBrowserDialogWrapper factory = new FolderBrowserDialogWrapper();
            Mock<CommonDialog> dialog = new Mock<CommonDialog>();

            // ACT
            factory.GetSelectedPathFromDialog(dialog.Object);

            // ASSERT (Handled in ExpectedException)
        }
    }
}
