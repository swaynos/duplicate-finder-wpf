using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinder.Utilities
{
    /// <summary>
    /// QualityComparer designed to handle comparing the Hash of two ScannedFile objects
    /// </summary>
    class ScannedFileHashComparer : EqualityComparer<byte[]>
    {
        public override bool Equals(byte[] first, byte[] second)
        {
            if (first == null || second == null)
            {
                // null == null returns true.
                // non-null == null returns false.
                return first == second;
            }
            if (ReferenceEquals(first, second))
            {
                return true;
            }
            if (first.Length != second.Length)
            {
                return false;
            }
            // Linq extension method is based on IEnumerable, must evaluate every item.
            return first.SequenceEqual(second);
        }

        public override int GetHashCode(byte[] obj)
        {
            // The requirement is to make this operation as fast as possible for very quick equality comparison.
            // If the int value of the first four bytes are equal than a full equality operation will 
            // eventually be used to compare the two byte arrays.
            byte[] objTemp;
            if (obj.Length < 4)
            {
                objTemp = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                for (int i = 0; i < obj.Length; i++)
                {
                    objTemp[i] = obj[i];
                }
            }
            else
            {
                objTemp = obj;
            }
            return BitConverter.ToInt32(objTemp, 0);
        }
    }
}
