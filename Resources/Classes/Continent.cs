using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContinentPro.Resources.Classes
{
    public class Continent
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContinentImageLocation { get; set; }
        public Point Size { get; set; }
        public Point Location { get; set; }

        public Continent()
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
