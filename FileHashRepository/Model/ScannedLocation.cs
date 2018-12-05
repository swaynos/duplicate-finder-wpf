using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public partial class ScannedLocation
    {
        public override bool Equals(object obj)
        {
            ScannedLocation otherScannedLocation = obj as ScannedLocation;

            bool isEqual = this.Path.Equals(otherScannedLocation.Path);

            return isEqual;
        }
    }
}
