using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository.Utilities
{
    internal class FileHash
    {
        private IFileSystem _fileSystem;

        public FileHash(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        ///  Computes SHA256 Checksum hash of provided file
        /// </summary>
        /// <param name="filePath">The file path where the file is located on disk</param>
        /// <returns>The Sha256 Checksum hash (32 bytes) of the file</returns>
        public async Task<byte[]> ComputeFileHashAsync(string filePath)
        {
            Task<byte[]> task = Task.Run(() =>
            {
                using (Stream stream = _fileSystem.File.OpenRead(filePath))
                {
                    var sha = new System.Security.Cryptography.SHA256Managed();
                    return sha.ComputeHash(stream);
                }
            });
            return await task;

            // The only way we reach here is if an exception occured
            throw task.Exception;
        }
    }
}
