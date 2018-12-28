using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinder.Utilities
{
    internal interface IProcess
    {
        Task<bool> StartAsync(string fileName);
    }
}
