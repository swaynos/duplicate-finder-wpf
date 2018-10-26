using Microsoft.VisualBasic.FileIO;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DuplicateFinder.Utilities
{
    internal class RecycleFile : IRecycleFile
    {
        private ILogger _logger;
        private IFileSystemWrapper _fileSystem;

        public async Task<bool> RecycleAsync(string filePath, bool suppressDialogs = false)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (suppressDialogs)
                    {
                        _fileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                    else
                    {
                        _fileSystem.DeleteFile(filePath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                    }
                });
                return true;
            }
            catch (FileNotFoundException fileNotFoundEx)
            {
                _logger.Warn(fileNotFoundEx);
                return true;
            }
            catch (OperationCanceledException cancelledEx)
            {
                _logger.Warn(cancelledEx, "The user cancelled the operation.");
                return false;
            }
        }

        public RecycleFile() : this(LogManager.GetCurrentClassLogger(), new FileSystemWrapper())
        {

        }

        public RecycleFile(ILogger logger, IFileSystemWrapper fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }
    }
}
