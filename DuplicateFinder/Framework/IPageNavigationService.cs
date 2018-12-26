using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinder.Framework
{
    public interface IPageNavigationService
    {
        bool Navigate(object root);
    }
}
