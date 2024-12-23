using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Entities
{
    public class CricketDartsPlayer : DartsPlayer
    {
        public int CurrentPoints { get; set; }
        public int Marks { get; set; }
        public int Rounds { get; set; }
    }
}
