using System.Collections.Generic;
using System.Linq;

namespace DuplicateFinder.Models
{
    public class ResultPageModel
    {
        public List<DuplicateResultModel> FoundDuplicates { get; set; }

        /// <summary>
        /// Will traverse the FoundDuplicates and return the LocationPath of each FileModel
        /// </summary>
        /// <returns>A collection of strings which contain all of the location paths in the FoundDuplicates</returns>
        public List<string> FlattenFoundDuplicates()
        {
            return FoundDuplicates.SelectMany(t => t.DuplicateFiles.Select(q => q.LocationPath)).ToList();
        }

        /// <summary>
        /// Traverses the FoundDuplicates and removes the FileModel of the given locationPath(s).
        /// Will remove all instances of FileModel that match. If a DuplicateResultModel is then
        /// empty of FileModels it will be removed from the model too.
        /// </summary>
        /// <param name="locationPath">The FileModel.LocationPath to search for.</param>
        public void RemoveFileModelFromModel(string locationPath)
        {
            // We can't modify the collection we're iterating through, so it was decided to
            // instead maintain a collection of references to objects we want to delete and then
            // iterate through those to delete from the original collection.
            List<DuplicateResultModel> duplicateResultModelRemovals = new List<DuplicateResultModel>();
            foreach (DuplicateResultModel duplicateResultModel in FoundDuplicates)
            {
                List<FileModel> fileModelRemovals = new List<FileModel>();
                foreach (FileModel fileModel in duplicateResultModel.DuplicateFiles)
                {
                    if (fileModel.LocationPath.Equals(locationPath))
                    {
                        fileModelRemovals.Add(fileModel);
                    }
                }
                foreach(FileModel removal in fileModelRemovals)
                {
                    duplicateResultModel.DuplicateFiles.Remove(removal);
                }
                if (duplicateResultModel.DuplicateFiles.Count == 0)
                {
                    duplicateResultModelRemovals.Add(duplicateResultModel);
                }
            }
            foreach(DuplicateResultModel removal in duplicateResultModelRemovals)
            {
                FoundDuplicates.Remove(removal);
            }
        }
    }
}
