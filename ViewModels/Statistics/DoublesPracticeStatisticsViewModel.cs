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
using System.Xml.Linq;

namespace DartsApp.ViewModels
{
    public partial class DoublesPracticeStatisticsViewModel : BaseViewModel
    {
        private readonly DbDataContext _dbDataContext;
        private readonly ITranslator _translator;


        [ObservableProperty]
        private List<DoublesPracticePlayerDTO> _legs;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private List<string> _dateTypes;

        [ObservableProperty]
        private string _selectedDateType;

        [ObservableProperty]
        private List<string> _scoringCaptions;

        [ObservableProperty]
        private List<string> _d20Values = ["0 %", "0 %"];

        [ObservableProperty]
        private List<string> _d16Values = ["0 %", "0 %"];

        [ObservableProperty]
        private List<string> _d10Values = ["0 %", "0 %"];

        [ObservableProperty]
        private List<string> _d8Values = ["0 %", "0 %"];

        [ObservableProperty]
        private List<string> _d4Values = ["0 %", "0 %"];

        public DoublesPracticeStatisticsViewModel(DbDataContext dbDataContext, ITranslator translator)
        {
            _dbDataContext = dbDataContext;
            _translator = translator;

            PropertyChanged += DoublesPracticeStatisticsViewModel_PropertyChanged;
        }

        public void Initialize(string name)
        {
            Name = name;

            DateTypes = [_translator.GetTranslation("X01StatisticsViewModel_Weekly"), _translator.GetTranslation("X01StatisticsViewModel_Monthly"), _translator.GetTranslation("X01StatisticsViewModel_Total")];
            ScoringCaptions = [_translator.GetTranslation("SelectTrainingView_Doubles"), _translator.GetTranslation("DoublesPracticeViewModel_0s")];

            SelectedDateType = "2";
        }
        private void DoublesPracticeStatisticsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName != nameof(SelectedDateType))
                    return;

                if (SelectedDateType == "2")
                    Legs = _dbDataContext.DoublesPracticeLegs.Where((x) => x.Name == Name).ToList();

                else if (SelectedDateType == "1")
                    Legs = _dbDataContext.DoublesPracticeLegs.Where((x) => x.Name == Name && x.PlayedDateTime > DateTime.Now.AddMonths(-1)).ToList();

                else
                    Legs = _dbDataContext.DoublesPracticeLegs.Where((x) => x.Name == Name && x.PlayedDateTime > DateTime.Now.AddDays(-7)).ToList();

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

            D20Values[0] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.D20Statistics.Split(' ')[1])) / (3 * Legs.Sum(x => int.Parse(x.D20Statistics.Split(' ')[0]))), 2).ToString() + " %";
            D20Values[1] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.D20Statistics.Split(' ')[2])) / (3 * Legs.Sum(x => int.Parse(x.D20Statistics.Split(' ')[0]))), 2).ToString() + " %";

            D16Values[0] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.D16Statistics.Split(' ')[1])) / (3 * Legs.Sum(x => int.Parse(x.D16Statistics.Split(' ')[0]))), 2).ToString() + " %";
            D16Values[1] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.D16Statistics.Split(' ')[2])) / (3 * Legs.Sum(x => int.Parse(x.D16Statistics.Split(' ')[0]))), 2).ToString() + " %";

            D10Values[0] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.D10Statistics.Split(' ')[1])) / (3 * Legs.Sum(x => int.Parse(x.D10Statistics.Split(' ')[0]))), 2).ToString() + " %";
            D10Values[1] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.D10Statistics.Split(' ')[2])) / (3 * Legs.Sum(x => int.Parse(x.D10Statistics.Split(' ')[0]))), 2).ToString() + " %";

            D8Values[0] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.D8Statistics.Split(' ')[1])) / (3 * Legs.Sum(x => int.Parse(x.D8Statistics.Split(' ')[0]))), 2).ToString() + " %";
            D8Values[1] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.D8Statistics.Split(' ')[2])) / (3 * Legs.Sum(x => int.Parse(x.D8Statistics.Split(' ')[0]))), 2).ToString() + " %";

            D4Values[0] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.D4Statistics.Split(' ')[1])) / (3 * Legs.Sum(x => int.Parse(x.D4Statistics.Split(' ')[0]))), 2).ToString() + " %";
            D4Values[1] = Math.Round((double)100 * Legs.Sum(x => int.Parse(x.D4Statistics.Split(' ')[2])) / (3 * Legs.Sum(x => int.Parse(x.D4Statistics.Split(' ')[0]))), 2).ToString() + " %";

            //To notify property changed
            D20Values = D20Values.ToArray().ToList();
            D16Values = D16Values.ToArray().ToList();
            D10Values = D10Values.ToArray().ToList();
            D8Values = D8Values.ToArray().ToList();
            D4Values = D4Values.ToArray().ToList();
        }
    }
}
