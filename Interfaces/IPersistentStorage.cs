using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Interfaces
{
    public interface IPersistentStorage
    {
        public string PlayerList { get; set; }
    }
}
