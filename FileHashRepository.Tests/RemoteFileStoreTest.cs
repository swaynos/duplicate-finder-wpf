using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileHashRepository;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FileHashRepository.Tests
{
    [TestClass]
    public class RemoteFileStoreTest
    {
        private byte[] _fileHash
        {
            get
            {
                return Encoding.ASCII.GetBytes("THIS_IS_A_FILE_HASH");
            }
        }
        private const string _remotePath = "..\\..\\Files\\Target";

        [TestCleanup]
        public void Cleanup()
        {
            ScannedFileStore.DirectoryGetFiles = Directory.GetFiles;
            ScannedFileStore.FileExists = File.Exists;

            ScannedFileStore.RemoveRemoteFilesByHash(_fileHash);
            string[] files = Directory.GetFiles(_remotePath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                byte[] hash;
                using (FileStream stream = File.OpenRead(file))
                {
                    var sha = new System.Security.Cryptography.SHA256Managed();
                    hash = sha.ComputeHash(stream);
                }
                ScannedFileStore.RemoveRemoteFilesByHash(hash);
            }
        }
        [TestMethod]
        public void InsertRemoteFile_InsertsNewEntry()
        {
            // ARRANGE
            RemoteFile file = new RemoteFile()
            {
                Hash = _fileHash,
                Name = "Foo.file",
                Path = "\\\\Some\\kind\\of\\path"
            };

            // ACT
            ScannedFileStore.InsertRemoteFile(file);

            // ASSERT
            Assert.IsTrue(ScannedFileStore.SearchRemoteFiles(_fileHash) > 0);
        }
        [TestMethod]
        public void RemoveRemoteFileByHash_RemovesEntry()
        {
            // ARRANGE
            RemoteFile file = new RemoteFile()
            {
                Hash = _fileHash,
                Name = "Foo.file",
                Path = "\\\\Some\\kind\\of\\path"
            };
            ScannedFileStore.InsertRemoteFile(file);

            // ACT
            ScannedFileStore.RemoveRemoteFilesByHash(_fileHash);

            // ASSERT
            Assert.IsTrue(ScannedFileStore.SearchRemoteFiles(_fileHash) == 0);
        }
        [TestMethod]
        public void SearchRemoteFilesByHash_ExistingFile_ReturnsGreaterThanZero()
        {
            // ARRANGE
            RemoteFile file = new RemoteFile()
            {
                Hash = _fileHash,
                Name = "Foo.file",
                Path = "\\\\Some\\kind\\of\\path"
            };
            ScannedFileStore.InsertRemoteFile(file);

            // ACT
            int result = ScannedFileStore.SearchRemoteFiles(_fileHash);

            // ASSERT
            Assert.IsTrue(result > 0);
        }
        [TestMethod]
        public void SearchRemoteFilesByHash_NonExistingFile_ReturnsZero()
        {
            // ARRANGE

            // ACT
            int result = ScannedFileStore.SearchRemoteFiles(_fileHash);

            // ASSERT
            Assert.IsTrue(result == 0);
        }
        [TestMethod]
        public void ScanRemoteDirectory_BuildsRemoteFileTable()
        {
            // ARRANGE
            // ACT
            ScannedFileStore.ScanRemoteDirectory(_remotePath);

            // ASSERT
            string[] files = Directory.GetFiles(_remotePath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                byte[] hash;
                using (FileStream stream = File.OpenRead(file))
                {
                    var sha = new System.Security.Cryptography.SHA256Managed();
                    hash = sha.ComputeHash(stream);
                }
                Assert.IsTrue(ScannedFileStore.SearchRemoteFiles(hash) > 0);
            }
        }
        [TestMethod]
        public void ScanRemoteDirectory_RebuildsRemoteFileTable()
        {
            // ARRANGE
            RemoteFile file = new RemoteFile()
            {
                Hash = _fileHash,
                Name = "Foo.file",
                Path = "\\\\Some\\kind\\of\\path"
            };
            ScannedFileStore.InsertRemoteFile(file);

            // ACT
            ScannedFileStore.ScanRemoteDirectory(_remotePath);

            // ASSERT
            Assert.IsTrue(ScannedFileStore.SearchRemoteFiles(_fileHash) == 0);
        }
        [TestMethod]
        public void ScanRemoteDirectory_ExecutesStatusCallback()
        {
            // ARRANGE
            bool callbackExecuted = false;

            // ACT
            ScannedFileStore.ScanRemoteDirectory(_remotePath, t =>
            {
                callbackExecuted = true;
            });

            // ASSERT
            Assert.IsTrue(callbackExecuted);
        }
        [TestMethod]
        public void ScanRemoteDirectory_ExecutesStatusCallback_OnlyOnPercentageChange()
        {
            // ARRANGE
            int percentageCount = -2; // Offset for Begin and End messages
            DirectoryInfo remotePath = new DirectoryInfo(GetTemporaryDirectory());
            string file = Path.Combine(_remotePath, "One.png");
            for (int i = 0; i < 1000; i++)
            {
                File.Copy(file, Path.Combine(remotePath.FullName, Path.GetRandomFileName()));
            }

            // ACT
            ScannedFileStore.ScanRemoteDirectory(remotePath.FullName, t =>
            {
                percentageCount++;
            });

            // ASSERT
            Assert.IsTrue(percentageCount == 100); // 100 percentage status changes

            // Cleanup
            remotePath.GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(t => t.IsReadOnly = false);
            remotePath.Delete(true);
        }
        [TestMethod]
        public void ScanRemoteDirectory_FileNotFound_PassesOverFile()
        {
            // ARRANGE
            int numberOfFilesScanned = 0;
            ScannedFileStore.DirectoryGetFiles = (string path, string searchPatther, System.IO.SearchOption searchOption) =>
            {
                return new string[] {
                    "foobar",
                    "foobar2"
                };
            };
            ScannedFileStore.FileExists = (string path) =>
            {
                numberOfFilesScanned++;
                return false;
            };

            // ACT
            ScannedFileStore.ScanRemoteDirectory(_remotePath);

            // ASSERT
            Assert.AreEqual(numberOfFilesScanned, 2, "The number of files scanned does not match what was expected.");
        }
        [TestMethod]
        public void ScanRemoteDirectory_FileNotFound_CreatesMessage()
        {
            // ARRANGE
            List<string> messages = new List<string>();
            ScannedFileStore.DirectoryGetFiles = (string path, string searchPatther, System.IO.SearchOption searchOption) =>
            {
                return new string[] {
                    "foobar"
                };
            };
            ScannedFileStore.FileExists = (string path) =>
            {
                return false;
            };

            // ACT
            ScannedFileStore.ScanRemoteDirectory(_remotePath, (message) =>
            {
                messages.Add(message);
            });

            // ASSERT
            Assert.IsTrue(messages.Contains("File not found foobar."), "The log message expected was not generated.");
        }
        [TestMethod]
        public void RescanRemoteDirectory_AddsNewFilestoRemoteFileTable()
        {
            // ARRANGE
            DirectoryInfo remotePath = new DirectoryInfo(GetTemporaryDirectory());
            string file = Path.Combine(_remotePath, "One.png");
            string addedFile = Path.Combine(_remotePath, "Dir\\Two.png");
            File.Copy(file, Path.Combine(remotePath.FullName, Path.GetRandomFileName()));
            ScannedFileStore.ScanRemoteDirectory(remotePath.FullName);
            File.Copy(addedFile, Path.Combine(remotePath.FullName, Path.GetRandomFileName()));

            // ACT
            ScannedFileStore.RescanRemoteDirectory(remotePath.FullName);

            // ASSERT
            byte[] hash;
            using (FileStream stream = File.OpenRead(addedFile))
            {
                var sha = new System.Security.Cryptography.SHA256Managed();
                hash = sha.ComputeHash(stream);
            }
            Assert.IsTrue(ScannedFileStore.SearchRemoteFiles(hash) > 0);

            // Cleanup
            remotePath.GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(t => t.IsReadOnly = false);
            remotePath.Delete(true);
        }
        [TestMethod]
        public void RescanRemoteDirectory_RemovesPurgedFilesFromRemoteFileTable()
        {
            // ARRANGE
            DirectoryInfo remotePath = new DirectoryInfo(GetTemporaryDirectory());
            string fileOne = Path.Combine(_remotePath, "One.png");
            string fileTwo = Path.Combine(_remotePath, "Dir\\Two.png");
            string fileTwoAtRemotePath = Path.Combine(remotePath.FullName, Path.GetRandomFileName());
            File.Copy(fileOne, Path.Combine(remotePath.FullName, Path.GetRandomFileName()));
            File.Copy(fileTwo, fileTwoAtRemotePath);
            ScannedFileStore.ScanRemoteDirectory(remotePath.FullName);
            File.SetAttributes(fileTwoAtRemotePath, ~FileAttributes.ReadOnly);
            File.Delete(fileTwoAtRemotePath);

            // ACT
            ScannedFileStore.RescanRemoteDirectory(remotePath.FullName);

            // ASSERT
            byte[] hash;
            using (FileStream stream = File.OpenRead(fileTwo))
            {
                var sha = new System.Security.Cryptography.SHA256Managed();
                hash = sha.ComputeHash(stream);
            }


            Assert.IsTrue(ScannedFileStore.SearchRemoteFiles(hash) == 0);

            // Cleanup
            remotePath.GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(t => t.IsReadOnly = false);
            remotePath.Delete(true);
        }
        [TestMethod]
        public void RescanRemoteDirectory_FileNotFound_PassesOverFile()
        {
            // ARRANGE
            int numberOfFilesScanned = 0;
            ScannedFileStore.DirectoryGetFiles = (string path, string searchPatther, System.IO.SearchOption searchOption) =>
            {
                return new string[] {
                    "foobar",
                    "foobar2"
                };
            };
            ScannedFileStore.FileExists = (string path) =>
            {
                numberOfFilesScanned++;
                return false;
            };

            // ACT
            ScannedFileStore.RescanRemoteDirectory(_remotePath);

            // ASSERT
            Assert.AreEqual(numberOfFilesScanned, 2, "The number of files scanned does not match what was expected.");
        }
        [TestMethod]
        public void RescanRemoteDirectory_FileNotFound_CreatesMessage()
        {
            // ARRANGE
            List<string> messages = new List<string>();
            ScannedFileStore.DirectoryGetFiles = (string path, string searchPatther, System.IO.SearchOption searchOption) =>
            {
                return new string[] {
                    "foobar"
                };
            };
            ScannedFileStore.FileExists = (string path) =>
            {
                return false;
            };

            // ACT
            ScannedFileStore.RescanRemoteDirectory(_remotePath, (message) =>
            {
                messages.Add(message);
            });

            // ASSERT
            Assert.IsTrue(messages.Contains("File not found foobar."), "The log message expected was not generated.");
        }
        [TestMethod]
        public void RescanRemoteDirectory_ExecutesStatusCallback()
        {
            // ARRANGE
            bool callbackExecuted = false;

            // ACT
            ScannedFileStore.RescanRemoteDirectory(_remotePath, t =>
            {
                callbackExecuted = true;
            });

            // ASSERT
            Assert.IsTrue(callbackExecuted);
        }
        [TestMethod]
        public void RescanRemoteDirectory_ExecutesStatusCallback_OnlyOnPercentageChange()
        {
            // ARRANGE
            int percentageCount = -2; // Offset for Begin and End messages
            DirectoryInfo remotePath = new DirectoryInfo(GetTemporaryDirectory());
            string file = Path.Combine(_remotePath, "One.png");
            for (int i = 0; i < 10; i++)
            {
                File.Copy(file, Path.Combine(remotePath.FullName, Path.GetRandomFileName()));
            }
            ScannedFileStore.ScanRemoteDirectory(remotePath.FullName);
            for (int i = 0; i < 1000; i++)
            {
                File.Copy(file, Path.Combine(remotePath.FullName, Path.GetRandomFileName()));
            }

            // ACT
            ScannedFileStore.RescanRemoteDirectory(remotePath.FullName, t =>
            {
                percentageCount++;
            });

            // ASSERT
            Assert.IsTrue(percentageCount == 100); // 100 percentage status changes

            // Cleanup
            remotePath.GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(t => t.IsReadOnly = false);
            remotePath.Delete(true);
        }

        private string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
