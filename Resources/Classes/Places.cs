using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContinentPro.Resources.Classes
{
    public struct Places
    {
        int X { get; set; }
        int Y { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string ImageLocation { get; set; }
        string SoundLocation { get; set; }

        Quiz[] quizzes { get; set; }
    }
}
