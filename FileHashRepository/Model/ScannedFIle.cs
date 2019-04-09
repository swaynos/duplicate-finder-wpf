using FileHashRepository.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public class ScannedFile
    {
        public byte[] Hash { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public long Length { get; set; }

        public override bool Equals(object obj)
        {
            ScannedFileHashComparer comparer = new ScannedFileHashComparer();
            ScannedFile otherScannedFile = obj as ScannedFile;

            bool isEqual = this.Name.Equals(otherScannedFile.Name)
                && this.Path.Equals(otherScannedFile.Path)
                && this.Length.Equals(otherScannedFile.Length)
                && comparer.Equals(this.Hash, otherScannedFile.Hash);

            return isEqual;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}
