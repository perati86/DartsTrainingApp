using DartsApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DartsApp.Services
{
    internal class PersistentStorageService : IPersistentStorage
    {
        public string PlayerList
        {
            get => Preferences.Get(nameof(PlayerList), string.Empty);
            set => Preferences.Set(nameof(PlayerList), value);
        }
    }
}
