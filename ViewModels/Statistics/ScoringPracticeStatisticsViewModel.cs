using banditoth.MAUI.Multilanguage.Interfaces;
using banditoth.MAUI.MVVM.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using DartsApp.Entities.DTO;
using DartsApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public partial class ScoringPracticeStatisticsViewModel : BaseViewModel
    {
        private readonly DbDataContext _dbDataContext;
        private readonly ITranslator _translator;


        [ObservableProperty]
        private List<ScoringPracticePlayerDTO> _legs;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private List<string> _dateTypes;

        [ObservableProperty]
        private string _selectedDateType;

        [ObservableProperty]
        private List<string> _scoringCaptions;

        [ObservableProperty]
        private List<string> _t20Values = ["0 %", "0 %"];

        [ObservableProperty]
        private List<string> _t19Values = ["0 %", "0 %"];

        [ObservableProperty]
        private List<string> _t18Values = ["0 %", "0 %"];

        public ScoringPracticeStatisticsViewModel(DbDataContext dbDataContext, ITranslator translator)
        {
            _dbDataContext = dbDataContext;
            _translator = translator;

            PropertyChanged += ScoringPracticeStatisticsViewModel_PropertyChanged;
        }

        public void Initialize(string name)
        {
            Name = name;

            DateTypes = [_translator.GetTranslation("X01StatisticsViewModel_Weekly"), _translator.GetTranslation("X01StatisticsViewModel_Monthly"), _translator.GetTranslation("X01StatisticsViewModel_Total")];
            ScoringCaptions = [_translator.GetTranslation("ScoringPracticeViewModel_Triple"), _translator.GetTranslation("ScoringPracticeViewModel_Sector")];

            SelectedDateType = "2";
        }
        private void ScoringPracticeStatisticsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName != nameof(SelectedDateType))
                    return;

                if (SelectedDateType == "2")
                    Legs = _dbDataContext.ScoringPracticeLegs.Where((x) => x.Name == Name).ToList();

                else if (SelectedDateType == "1")
                    Legs = _dbDataContext.ScoringPracticeLegs.Where((x) => x.Name == Name && x.PlayedDateTime > DateTime.Now.AddMonths(-1)).ToList();

                else
                    Legs = _dbDataContext.ScoringPracticeLegs.Where((x) => x.Name == Name && x.PlayedDateTime > DateTime.Now.AddDays(-7)).ToList();

                UpdateStatistics();
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void UpdateStatistics()
        {
            if (Legs?.Any() != true)
                return;

            T20Values[0] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.T20Statistics.Split(' ')[1])) / (3 * Legs.Sum(x => int.Parse(x.T20Statistics.Split(' ')[0]))), 2).ToString() + " %";
            T20Values[1] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.T20Statistics.Split(' ')[2])) / (3 * Legs.Sum(x => int.Parse(x.T20Statistics.Split(' ')[0]))), 2).ToString() + " %";

            T19Values[0] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.T19Statistics.Split(' ')[1])) / (3 * Legs.Sum(x => int.Parse(x.T19Statistics.Split(' ')[0]))), 2).ToString() + " %";
            T19Values[1] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.T19Statistics.Split(' ')[2])) / (3 * Legs.Sum(x => int.Parse(x.T19Statistics.Split(' ')[0]))), 2).ToString() + " %";

            T18Values[0] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.T18Statistics.Split(' ')[1])) / (3 * Legs.Sum(x => int.Parse(x.T18Statistics.Split(' ')[0]))), 2).ToString() + " %";
            T18Values[1] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.T18Statistics.Split(' ')[2])) / (3 * Legs.Sum(x => int.Parse(x.T18Statistics.Split(' ')[0]))), 2).ToString() + " %";

            //To notify property changed
            T20Values = T20Values.ToArray().ToList();
            T19Values = T19Values.ToArray().ToList();
            T18Values = T18Values.ToArray().ToList();
        }
    }
}
