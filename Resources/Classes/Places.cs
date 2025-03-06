using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContinentPro.Resources.Classes
{
    public struct Places
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageLocation { get; set; }
        public string SoundLocation { get; set; }

        public Quiz[] Quizzes { get; set; }
    }
}
