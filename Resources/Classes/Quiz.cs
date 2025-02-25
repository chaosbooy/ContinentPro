using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContinentPro.Resources.Classes
{
    public struct Quiz
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public int Answer { get; set; }

        public string ImageLocation { get; set; }
    }
}
