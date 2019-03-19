using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public partial class ScannedLocation
    {
        public string Path { get; set; }

        public override bool Equals(object obj)
        {
            ScannedLocation otherScannedLocation = obj as ScannedLocation;

            bool isEqual = this.Path.Equals(otherScannedLocation.Path);

            return isEqual;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}
