using System.Threading.Tasks;

namespace DuplicateFinder.Utilities
{
    internal interface IRecycleFile
    {
        /// <summary>
        /// Recycle the file given and return true or false whether the file was sent to the recycling bin
        /// </summary>
        Task<bool> RecycleAsync(string filePath, bool suppressDialogs = false);
    }
}
