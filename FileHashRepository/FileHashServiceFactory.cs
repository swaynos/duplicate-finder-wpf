using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    /// <summary>
    /// Internal implementation of the FileHashServiceFactory
    /// </summary>
    internal class FileHashServiceFactory : IFileHashServiceFactory
    {
        /// <summary>
        /// Creates a new instance of FileHashService
        /// </summary>
        /// <returns>A newly contstructed FileHashService</returns>
        public IFileHashService GetFileHashService()
        {
            FileHashEntities dbContext = new FileHashEntities();
            return new FileHashService(dbContext);
        }
    }
}
