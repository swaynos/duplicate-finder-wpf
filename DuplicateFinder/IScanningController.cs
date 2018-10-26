using DuplicateFinder.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace DuplicateFinder
{
    public interface IScanningController
    {
        Task BeginScanAsync(RangeBase control);

        Task<List<DuplicateResultModel>> RetrieveDuplicatesAsync();
    }
}
