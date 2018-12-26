using System;

namespace DuplicateFinder.Models
{
    public class ScanLocation : IEquatable<ScanLocation>
    {
        public ScanLocation(string path)
        {
            this.Path = path;
        }

        public string Path { get; set; }

        public bool Equals(ScanLocation other)
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
            return string.Equals(this.Path, other.Path, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ScanLocation);
        }

        public override int GetHashCode()
        {
            if (Path == null) return 0;
            return Path.GetHashCode();
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
