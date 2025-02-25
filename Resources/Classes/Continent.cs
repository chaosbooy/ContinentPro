using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContinentPro.Resources.Classes
{
    public struct Continent
    {
        string Name { get; set; }
        string Description { get; set; }
        string ContinentImageLocation { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        int Latitude { get; set; }
        int Longitude { get; set; }
        Places places { get; set; }


        public override string ToString()
        {
            return Name;
        }
    }
}
