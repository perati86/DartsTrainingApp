using DartsApp.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Entities
{
    public class CricketGame : GameFormat
    {
        public bool HasPoints { get; set; }
        public CricketGameType Type { get; set; }
    }
}
