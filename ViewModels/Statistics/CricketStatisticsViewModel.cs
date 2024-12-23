using banditoth.MAUI.Multilanguage.Interfaces;
using banditoth.MAUI.MVVM.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using DartsApp.Entities;
using DartsApp.Entities.DTO;
using DartsApp.Services;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public partial class CricketStatisticsViewModel : BaseViewModel
    {
        private readonly DbDataContext _dbDataContext;
        private readonly ITranslator _translator;

        [ObservableProperty]
        private CricketDartsPlayer _player;

        [ObservableProperty]
        private List<CricketPlayerDTO> _legs;

        [ObservableProperty]
        private List<string> _dateTypes;

        [ObservableProperty]
        private string _selectedDateType;

        [ObservableProperty]
        private List<string> _legCaptions;

        [ObservableProperty]
        private List<string> _legValues = ["0", "0", "0,0%"];

        [ObservableProperty]
        private List<string> _scoringCaptions;

        [ObservableProperty]
        private List<string> _markValues = ["0,00", "0,00", "0,00"];

        [ObservableProperty]
        private List<string> _pointValues = ["0", "0", "0"];

        [ObservableProperty]
        private List<string> _roundValues = ["0,00", "0,00", "0,00"];


        public CricketStatisticsViewModel(DbDataContext dbDataContext, ITranslator translator)
        {
            _dbDataContext = dbDataContext;
            _translator = translator;

            PropertyChanged += CricketStatisticsViewModel_PropertyChanged;
        }

        private void CricketStatisticsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName != nameof(SelectedDateType))
                    return;

                if (SelectedDateType == "2")
                    Legs = _dbDataContext.CricketLegs.Where((x) => x.Name == Player.Name).ToList();

                else if (SelectedDateType == "1")
                    Legs = _dbDataContext.CricketLegs.Where((x) => x.Name == Player.Name && x.PlayedDateTime > DateTime.Now.AddMonths(-1)).ToList();

                else
                    Legs = _dbDataContext.CricketLegs.Where((x) => x.Name == Player.Name && x.PlayedDateTime > DateTime.Now.AddDays(-7)).ToList();

                UpdateStatistics();
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        public void Initialize(CricketDartsPlayer player)
        {
            Player = player;

            DateTypes = [_translator.GetTranslation("X01StatisticsViewModel_Weekly"), _translator.GetTranslation("X01StatisticsViewModel_Monthly"), _translator.GetTranslation("X01StatisticsViewModel_Total")];
            LegCaptions = [_translator.GetTranslation("SelectGameView_Legs"), _translator.GetTranslation("X01StatisticsViewModel_LegsWon"), $"{_translator.GetTranslation("X01StatisticsViewModel_LegsWon")} %"];
            ScoringCaptions = [_translator.GetTranslation("CricketStatisticsViewModel_Minimum"), _translator.GetTranslation("CricketStatisticsViewModel_Average"), _translator.GetTranslation("CricketStatisticsViewModel_Maximum")];

            SelectedDateType = "2";
        }

        private void UpdateStatistics()
        {
            if (Legs?.Any() != true)
                return;

            LegValues[0] = Legs.Count.ToString();
            LegValues[1] = Legs.Where(x => x.LegWon).Count().ToString();
            LegValues[2] = Math.Round(double.Parse(LegValues[1]) * 100 / Legs.Count, 2).ToString() + "%";

            var bestMarksPerRound = Legs.OrderByDescending(x => x.Marks / x.Rounds).First();
            var worstMarksPerRound = Legs.OrderBy(x => x.Marks / x.Rounds).First();

            MarkValues[0] = Math.Round((double)worstMarksPerRound.Marks / worstMarksPerRound.Rounds, 2).ToString();
            MarkValues[1] = Math.Round(Legs.Average(x => x.Marks / x.Rounds), 2).ToString();
            MarkValues[2] = Math.Round((double)bestMarksPerRound.Marks / bestMarksPerRound.Rounds, 2).ToString();

            PointValues[0] = Math.Round((double)Legs.OrderBy(x => x.Points).First().Points, 2).ToString();
            PointValues[1] = Math.Round(Legs.Average(x => x.Points), 2).ToString();
            PointValues[2] = Math.Round((double)Legs.OrderByDescending(x => x.Points).First().Points, 2).ToString();

            RoundValues[0] = Math.Round((double)Legs.Where(x => x.LegWon).OrderBy(x => x.Rounds).First().Rounds, 2).ToString();
            RoundValues[1] = Math.Round(Legs.Where(x => x.LegWon).Average(x => x.Rounds), 2).ToString();
            RoundValues[2] = Math.Round((double)Legs.Where(x => x.LegWon).OrderByDescending(x => x.Rounds).First().Rounds, 2).ToString();

            //To notify property changed
            LegValues = LegValues.ToArray().ToList();
            MarkValues = MarkValues.ToArray().ToList();
            PointValues = PointValues.ToArray().ToList();
            RoundValues = RoundValues.ToArray().ToList();
        }
    }
}
