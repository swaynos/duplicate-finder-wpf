using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinder.Models
{
    public class HomePageModel
    {
        public HomePageModel()
        {
            this.Locations = new List<string>();
        }

        public List<string> Locations { get; set; }
    }
}
