using banditoth.MAUI.Multilanguage.Interfaces;
using banditoth.MAUI.MVVM.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using DartsApp.Entities;
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
    public partial class X01StatisticsViewModel : BaseViewModel
    {
        private readonly DbDataContext _dbDataContext;
        private readonly ITranslator _translator;

        [ObservableProperty]
        private X01HumanPlayer _player;

        [ObservableProperty]
        private List<X01PlayerDTO> _legs;

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
        private List<string> _scoringValues = ["0,00", "0,00", "0"];

        [ObservableProperty]
        private List<string> _pointCaptions = ["60+", "100+", "140+", "180"];

        [ObservableProperty]
        private List<string> _pointValues = ["0", "0", "0", "0"];

        [ObservableProperty]
        private List<string> _checkoutCaptions;

        [ObservableProperty]
        private List<string> _checkoutValues = ["0", "0,00%", "0"];


        public X01StatisticsViewModel(DbDataContext dbDataContext, ITranslator translator)
        {
            _dbDataContext = dbDataContext;
            _translator = translator;

            PropertyChanged += X01StatisticsViewModel_PropertyChanged;
        }

        private void X01StatisticsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName != nameof(SelectedDateType))
                    return;

                if (SelectedDateType == "2")
                    Legs = _dbDataContext.X01Legs.Where((x) => x.Name == Player.Name).ToList();

                else if (SelectedDateType == "1")
                    Legs = _dbDataContext.X01Legs.Where((x) => x.Name == Player.Name && x.PlayedDateTime > DateTime.Now.AddMonths(-1)).ToList();

                else
                    Legs = _dbDataContext.X01Legs.Where((x) => x.Name == Player.Name && x.PlayedDateTime > DateTime.Now.AddDays(-7)).ToList();

                UpdateStatistics();
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }

        public void Initialize(X01HumanPlayer player)
        {
            Player = player;

            DateTypes = [_translator.GetTranslation("X01StatisticsViewModel_Weekly"), _translator.GetTranslation("X01StatisticsViewModel_Monthly"), _translator.GetTranslation("X01StatisticsViewModel_Total")];
            LegCaptions = [_translator.GetTranslation("SelectGameView_Legs"), _translator.GetTranslation("X01StatisticsViewModel_LegsWon"), $"{_translator.GetTranslation("X01StatisticsViewModel_LegsWon")} %"];
            ScoringCaptions = [_translator.GetTranslation("X01StatisticsViewModel_9DartAverage"), _translator.GetTranslation("X01StatisticsViewModel_3DartAverage"), _translator.GetTranslation("X01StatisticsViewModel_HighestScore")];
            CheckoutCaptions = [_translator.GetTranslation("X01StatisticsViewModel_HighestCheckout"), $"{_translator.GetTranslation("X01StatisticsView_Checkout")} %", _translator.GetTranslation("X01StatisticsViewModel_BestLeg")];

            SelectedDateType = "2";
        }

        private void UpdateStatistics()
        {
            if (Legs?.Any() != true)
                return;

            LegValues[0] = Legs.Count.ToString();
            LegValues[1] = Legs.Where(x => x.LegWon).Count().ToString();
            LegValues[2] = Math.Round(double.Parse(LegValues[1]) * 100 / Legs.Count, 2).ToString() + "%";

            var maxScore = Legs.Max(x => x.HighestScoreThisLeg);

            ScoringValues[0] = Math.Round((double)Legs.Sum(x => x.First9DartsPoints) / (Legs.Count * 3), 2).ToString();
            ScoringValues[1] = Math.Round(Legs.Sum(x => (double)x.Points) / Legs.Sum(x => x.DartsThrown) * 3 , 2).ToString();
            ScoringValues[2] = maxScore.ToString();
            ScoringValues[2] += $" ({Legs.Where(x => x.HighestScoreThisLeg == maxScore).Sum(x => x.HighestScoreCountThisLeg)})";

            for (int i = 0; i < Legs[Legs.Count - 1].HighThrowCounts.Length; i++)
            {
                double sum = Legs.Sum(x => x.HighThrowCounts[i]);

                PointValues[i] = sum.ToString();

                if (sum > 9)
                    PointValues[i] += $" ({Math.Round(sum * 100 / (Legs.Sum(x => x.DartsThrown) / 3))}%)";
            }

            CheckoutValues[0] = Legs.Where(x => x.LegWon).Max(x => x.Checkout).ToString();
            CheckoutValues[1] = Math.Round((double)Legs.Where(x => x.LegWon).Count() * 100 / Legs.Sum(x => x.CheckOutDartsThrown), 2).ToString() + "%";
            CheckoutValues[2] = Legs.Where((x) => x.LegWon).Min(x => x.DartsThrown).ToString();

            //To notify property changed
            LegValues = LegValues.ToArray().ToList();
            ScoringValues = ScoringValues.ToArray().ToList();
            PointValues = PointValues.ToArray().ToList();
            CheckoutValues = CheckoutValues.ToArray().ToList();
        }
    }
}
