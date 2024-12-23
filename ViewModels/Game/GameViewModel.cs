using banditoth.MAUI.MVVM.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DartsApp.Entities;
using DartsApp.Enumerations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public abstract partial class GameViewModel : BaseViewModel
    {
        [ObservableProperty]
        private int _currentPlayerIndex;

        [ObservableProperty]
        internal GameFormat _gameParameters;

        public int LegCount = 0;

        public GameViewModel()
        {

        }

        public void Initialize()
        {

        }

        internal abstract void InitializeNewLeg(bool setEnded);

        

        internal string ConvertThrowToString(int sector, ScoreType scoreType)
        {
            string convertedString = scoreType == ScoreType.Triple ? "T" : scoreType == ScoreType.Double ? "D" : "";

            convertedString += sector.ToString();

            return convertedString;
        }

    }
}
