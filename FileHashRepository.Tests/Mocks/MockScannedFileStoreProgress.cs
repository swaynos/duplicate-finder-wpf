using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository.Tests.Mocks
{
    class MockScannedFileStoreProgress : IProgress<int>
    {
        public List<int> ReportedValues { get; set; }

        public MockScannedFileStoreProgress()
        {
            ReportedValues = new List<int>();
        }

        public void Report(int value)
        {
            ReportedValues.Add(value);
        }
    }
}
