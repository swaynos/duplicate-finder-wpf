using System;
using System.Threading.Tasks;
using DuplicateFinder.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System.IO;

namespace DuplicateFinder.Tests
{
    [TestClass]
    public class RecycleFileTest
    {
        [TestMethod]
        public async Task RecycleAsync_ThrownFileNotFoundException_WillLogWarning()
        {
            // ARRANGE
            string exceptionMessage = string.Empty;
            Mock<IFileSystemWrapper> fileSystem = new Mock<IFileSystemWrapper>();
            Mock<ILogger> logger = new Mock<ILogger>();
            RecycleFile recycle = new RecycleFile(logger.Object, fileSystem.Object);
            fileSystem.Setup(t => t.DeleteFile(It.IsAny<string>(), It.IsAny<UIOption>(), It.IsAny<RecycleOption>()))
                .Throws(new FileNotFoundException("test"));
            logger.Setup(t => t.Warn(It.IsAny<Exception>())).Callback<Exception>(ex =>
            {
                exceptionMessage = ex.Message;
            });

            // ACT
            bool result = await recycle.RecycleAsync(@"C:\foo\bar");

            // ASSERT
            Assert.AreEqual("test", exceptionMessage);
        }

        [TestMethod]
        public async Task RecycleAsync_OperationCanceledException_WillLogWarning()
        {
            // ARRANGE
            string exceptionMessage = null;
            Mock<IFileSystemWrapper> fileSystem = new Mock<IFileSystemWrapper>();
            Mock<ILogger> logger = new Mock<ILogger>();
            RecycleFile recycle = new RecycleFile(logger.Object, fileSystem.Object);
            fileSystem.Setup(t => t.DeleteFile(It.IsAny<string>(), It.IsAny<UIOption>(), It.IsAny<RecycleOption>()))
                .Throws(new OperationCanceledException("test"));
            logger.Setup(t => t.Warn(It.IsAny<Exception>(), It.IsAny<string>())).Callback<Exception, string>((ex, m) =>
            {
                exceptionMessage = ex.Message;
            });

            // ACT
            bool result = await recycle.RecycleAsync(@"C:\foo\bar");

            // ASSERT
            Assert.IsNotNull(exceptionMessage);
        }

        [TestMethod]
        public async Task RecycleAsync_SuppressRecycleFileDialog_PassesCorrectValues()
        {
            Mock<IFileSystemWrapper> fileSystem = new Mock<IFileSystemWrapper>();
            Mock<ILogger> logger = new Mock<ILogger>();
            RecycleFile recycle = new RecycleFile(logger.Object, fileSystem.Object);

            // ACT
            bool result = await recycle.RecycleAsync(@"C:\foo\bar", true);

            // ASSERT
            fileSystem.Verify(t => t.DeleteFile(It.IsAny<string>(),
                It.Is<UIOption>(ui => ui == UIOption.OnlyErrorDialogs),
                It.Is<RecycleOption>(r => r == RecycleOption.SendToRecycleBin)));
        }
    }
}
