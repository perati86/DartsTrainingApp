using banditoth.MAUI.Multilanguage.Interfaces;
using banditoth.MAUI.MVVM.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DartsApp.Entities.DTO;
using DartsApp.Enumerations;
using DartsApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public partial class ScoringPracticeViewModel : BaseViewModel
    {
        private readonly DbDataContext _dbDataContext;
        private readonly ITranslator _translator;


        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsScoringPractice))]
        private PracticeType _practiceType;

        [ObservableProperty]
        private int[] _currentScores = [0, 0];

        [ObservableProperty]
        private List<int> _recentScores = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TargetHitPercent), nameof(SectorHitPercent), nameof(CurrentTarget), nameof(SecondHitProperty))]
        private int _currentRound = 1;

        [ObservableProperty]
        private int _maxRound = 30;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TargetHitPercent), nameof(SectorHitPercent))]
        private int _targetHits;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SectorHitPercent), nameof(SecondHitProperty))]
        private int _sectorHits;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SectorHitPercent), nameof(SecondHitProperty))]
        private int _missHits;

        [ObservableProperty]
        private Dictionary<string, double> _targetList;

        [ObservableProperty]
        private Dictionary<string, int[]> _results = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentTarget))]
        private List<string> _targetsForRounds = [];

        [ObservableProperty]
        private string[] _keyboardOptions;


        public bool IsScoringPractice => PracticeType == PracticeType.Scoring;
        public string CurrentTarget => TargetsForRounds.Count > 0 && CurrentRound <= MaxRound ? TargetsForRounds[CurrentRound - 1] : "";

        public double TargetHitPercent => TargetHits == 0 ? 0.00 : Math.Round((double)TargetHits * 100 / ((CurrentRound - 1) * 3), 2);

        public double SectorHitPercent => SectorHits == 0 ? 0.00 : Math.Round((double)SectorHits * 100 / ((CurrentRound - 1) * 3), 2);

        public double MissHitPercent => MissHits == 0 ? 0.00 : Math.Round((double)MissHits * 100 / ((CurrentRound - 1) * 3), 2);

        public double SecondHitProperty => PracticeType == PracticeType.Scoring ? SectorHitPercent : MissHitPercent;


        public ScoringPracticeViewModel(DbDataContext dbDataContext, ITranslator translator)
        {
            _dbDataContext = dbDataContext;
            _translator = translator;
        }

        public void Initialize(PracticeType practiceType, string name)
        {
            PracticeType = practiceType;
            Name = name;

            if (PracticeType == PracticeType.Scoring)
            {
                KeyboardOptions = [_translator.GetTranslation("ScoringPracticeViewModel_Triple"), _translator.GetTranslation("ScoringPracticeViewModel_Single")];
                TargetList = new Dictionary<string, double>()
                {
                    {"T20", 0.7 },
                    {"T19", 0.9 },
                    {"T18", 1 }
                };
            }
            else
            {
                KeyboardOptions = [_translator.GetTranslation("ScoringPracticeViewModel_Double"), "0"];

                TargetList = new Dictionary<string, double>()
                {
                    {"D20", 0.35 },
                    {"D16", 0.7 },
                    {"D10", 0.8 },
                    {"D8", 0.9 },
                    {"D4", 1 },
                };
            }

            InitializeTargets();

            for (int i = 0; i < TargetList.Count; i++)
            {
                var key = TargetList.ElementAt(i).Key;

                Results.Add(key, new int[3]);
                Results[key][0] = TargetsForRounds.Where(x => x == key).Count();
            }

            OnPropertyChanged(nameof(CurrentTarget));
        }

        [RelayCommand]
        private async Task ScoreTyped(string sector)
        {
            if (int.TryParse(sector, out int index) != true || CurrentScores.Sum() > 2)
                return;

            CurrentScores[index]++;
            RecentScores.Add(index);

            CurrentScores = CurrentScores.ToList().ToArray();
        }

        [RelayCommand]
        private async Task SubmitScore()
        {
            CurrentRound++;

            TargetHits += CurrentScores[0];
            MissHits += CurrentScores[1];
            SectorHits += CurrentScores.Sum();

            Results[TargetsForRounds[CurrentRound - 2]][1] += CurrentScores[0];
            Results[TargetsForRounds[CurrentRound - 2]][2] += CurrentScores.Sum();

            if (CurrentRound > MaxRound)
            {
                await Application.Current.MainPage.DisplayAlert(_translator.GetTranslation("ScoringPracticeViewModel_Congratulations"),
                    _translator.GetTranslation("ScoringPracticeViewModel_FinishedTraining"), "OK");

                await SaveResults();
            }

            CurrentScores = [0, 0];
            RecentScores = [];

        }

        [RelayCommand]
        private async Task RemoveScore()
        {
            if (RecentScores.Any() != true)
                return;

            var indexToRemove = RecentScores[RecentScores.Count - 1];

            CurrentScores[indexToRemove]--;
            RecentScores.RemoveAt(RecentScores.Count - 1);

            //To notify usercontrol about change
            CurrentScores = CurrentScores.ToList().ToArray();
        }

        private void InitializeTargets()
        {
            Random rn = new Random();

            for (int i = 0; i < MaxRound; i++)
            {
                rn = new Random(rn.Next());

                var generatedNumber = rn.NextDouble();

                for (int j = 0; j < TargetList.Count; j++)
                {
                    if (generatedNumber <= TargetList.ElementAt(j).Value)
                    {
                        TargetsForRounds.Add(TargetList.ElementAt(j).Key);
                        break;
                    }
                }
            }
        }

        private async Task SaveResults()
        {
            try
            {
                if (IsScoringPractice)
                {
                    var playerDTO = new ScoringPracticePlayerDTO
                    {
                        ID = Guid.NewGuid(),
                        Name = Name,
                        T20Statistics = $"{Results["T20"][0]} {Results["T20"][1]} {Results["T20"][2]}",
                        T19Statistics = $"{Results["T19"][0]} {Results["T19"][1]} {Results["T19"][2]}",
                        T18Statistics = $"{Results["T18"][0]} {Results["T18"][1]} {Results["T18"][2]}",
                        PlayedDateTime = DateTime.Now
                    };

                    _dbDataContext.Add(playerDTO);
                }
                else
                {
                    var playerDTO = new DoublesPracticePlayerDTO
                    {
                        ID = Guid.NewGuid(),
                        Name = Name,
                        D20Statistics = $"{Results["D20"][0]} {Results["D20"][1]} {Results["D20"][2]}",
                        D16Statistics = $"{Results["D16"][0]} {Results["D16"][1]} {Results["D16"][2]}",
                        D10Statistics = $"{Results["D10"][0]} {Results["D10"][1]} {Results["D10"][2]}",
                        D8Statistics = $"{Results["D8"][0]} {Results["D8"][1]} {Results["D8"][2]}",
                        D4Statistics = $"{Results["D4"][0]} {Results["D4"][1]} {Results["D4"][2]}",
                        PlayedDateTime = DateTime.Now
                    };

                    _dbDataContext.Add(playerDTO);
                }
                
                await _dbDataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message + ex.StackTrace, "OK");
            }
        }
    }
}
