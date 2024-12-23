using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Entities
{
    public class DartsPlayer
    {
        public string Name { get; set; }
        public int Sets { get; set; }
        public int Legs { get; set; }
    }
}
