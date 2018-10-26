using Microsoft.VisualBasic.FileIO;

namespace DuplicateFinder.Utilities
{
    /// <summary>
    /// The implementation of IFileSystemWrapper
    /// </summary>
    internal class FileSystemWrapper : IFileSystemWrapper
    {
        public void DeleteFile(string file, UIOption showUi, RecycleOption recycle)
        {
            FileSystem.DeleteFile(file, showUi, recycle);
        }
    }
}
