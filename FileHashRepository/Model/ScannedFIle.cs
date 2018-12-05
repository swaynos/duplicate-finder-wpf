using FileHashRepository.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public partial class ScannedFile
    {
        public override bool Equals(object obj)
        {
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();
            ScannedFile otherScannedFile = obj as ScannedFile;

            bool isEqual = this.Name.Equals(otherScannedFile.Name);
            isEqual &= this.Path.Equals(otherScannedFile.Path);
            isEqual &= comparer.Equals(this.Hash, otherScannedFile.Hash);

            return isEqual;
        }
    }
}
