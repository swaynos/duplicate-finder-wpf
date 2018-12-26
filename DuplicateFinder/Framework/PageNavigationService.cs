using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace DuplicateFinder.Framework
{
    public class PageNavigationService : IPageNavigationService
    {
        private NavigationService NavigationService;

        public PageNavigationService(NavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        public bool Navigate(object root)
        {
            return NavigationService.Navigate(root);
        }
    }
}
