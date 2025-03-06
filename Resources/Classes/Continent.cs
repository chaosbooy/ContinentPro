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
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContinentImageLocation { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public Places[] PlacesInfo { get; set; }


        public override string ToString()
        {
            return Name;
        }
    }
}
