using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinder.Models
{
    public class ScanResult
    {
        public string FilePath { get; set; }

        public byte[] Hash { get; set; }

        public bool IsSelected { get; set; }

        // ToDo: Do something with this
        public string Background
        {
            get
            {
                return "Transparent";
            }
        }

        public bool Equals(ScanResult other)
        {
            if (this == null && other == null)
            {
                return true;
            }
            if (this == null && other != null)
            {
                return false;
            }
            if (this != null && other == null)
            {
                return false;
            }
            return string.Equals(this.FilePath, other.FilePath, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ScanResult);
        }

        public override int GetHashCode()
        {
            if (FilePath == null) return 0;
            return FilePath.GetHashCode();
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
