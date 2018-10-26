using Microsoft.VisualBasic.FileIO;

namespace DuplicateFinder.Utilities
{
    /// <summary>
    /// <para>Iterface wrapper of Microsoft.VisualBasic.FileIO.FileSystem</para>
    /// <para>Allows for dependency injection in Unit Tests</para>
    /// </summary>
    internal interface IFileSystemWrapper
    {
        /// <summary>
        /// Call a DeleteFile operation with the provided UI and Reyclce options.
        /// </summary>
        void DeleteFile(string file, UIOption showUi, RecycleOption recycle);
    }
}
