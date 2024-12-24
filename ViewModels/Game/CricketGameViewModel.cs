using banditoth.MAUI.Multilanguage.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DartsApp.Entities;
using DartsApp.Entities.DTO;
using DartsApp.Enumerations;
using DartsApp.Services;
using DartsApp.Views;
using Mopups.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public partial class CricketGameViewModel : GameViewModel
    {
        private readonly ITranslator _translator;
        private readonly DbDataContext _dbDataContext;

        private int[] DartsTable = [20, 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11, 14, 9, 12, 5];

        [ObservableProperty]
        private Guid _matchID;

        [ObservableProperty]
        private List<CricketDartsPlayer> _playerList = [];

        [ObservableProperty]
        private CricketGame _gameParameters;

        [ObservableProperty]
        private List<List<int>> _scores = [];

        [ObservableProperty]
        private string[] _sectors;

        [ObservableProperty]
        private string[] _botTargets = new string[3];

        [ObservableProperty]
        private List<int> _currentPlayerScores = [0, 0, 0, 0, 0, 0, 0];

        [ObservableProperty]
        private List<int> _recentScores = [];

        [ObservableProperty]
        private List<int> _disabledSectors = [];

        [ObservableProperty]
        private Color[] _botThrowBackgrounds = [Colors.Black, Colors.Black, Colors.Black];

        [ObservableProperty]
        private bool _isMatchOver;

        public string CurrentPlayerName => PlayerList?.Any() == true ? PlayerList[CurrentPlayerIndex].Name : "";

        public CricketGameViewModel(ITranslator translator, DbDataContext dbDataContext)
        {
            _translator = translator;
            _dbDataContext = dbDataContext;

            PropertyChanged += CricketGameViewModel_PropertyChanged;
        }

        private async void CricketGameViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentPlayerIndex) && PlayerList[CurrentPlayerIndex] is CricketDartsBot && IsMatchOver == false)
            {
                if (MopupService.Instance.PopupStack.Count > 0)
                {
                    await Task.Delay(1000);

                    await MopupService.Instance.PopAllAsync();
                }
                await Task.Delay(500);

                await BotThrow();
            }
        }

        public void Initialize(CricketGame game, List<CricketDartsPlayer> players)
        {
            GameParameters = game;
            PlayerList = players;
            MatchID = Guid.NewGuid();

            InitializeSectors();

            for (int i = 0; i < players.Count; i++)
            {
                Scores.Add(new List<int> { 0, 0, 0, 0, 0, 0, 0 });
            }

            if (PlayerList[0] is CricketDartsBot)
                _ = BotThrow();
        }

        private void InitializeSectors()
        {
            if (GameParameters.Type != CricketGameType.Random_numbers)
            {
                Sectors = ["20", "19", "18", "17", "16", "15", "Bull"];
                return;
            }

            Random rn = new Random();
            int[] sectorIndexes = new int[6];

            for (int i = 0; i < 6; i++)
            {
                var number = -1;

                while (number < 0 || sectorIndexes.Contains(number) || sectorIndexes.Contains(number - 1) || sectorIndexes.Contains(number + 1)
                    || number == 0 && sectorIndexes.Contains(sectorIndexes.Length - 1) || number == sectorIndexes.Length - 1 && sectorIndexes.Contains(0))
                {
                    number = rn.Next(0, 20);
                }

                sectorIndexes[i] = number;
                Sectors[i] = sectorIndexes[i].ToString();
            }

            Sectors[6] = "Bull";
        }

        [RelayCommand]
        private async Task ScoreTyped(string sectorIndex)
        {
            try
            {
                if (int.TryParse(sectorIndex, out int index) != true)
                    return;

                if (GetUsedDartsCount(index) > 3)
                    return;

                CurrentPlayerScores[index]++;
                RecentScores.Add(index);
                CheckDisabledSectors();

                //To notify usercontrol about change
                CurrentPlayerScores = CurrentPlayerScores.ToArray().ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        [RelayCommand]
        private async Task ScoreSubmitted()
        {
            try
            {
                PlayerList[CurrentPlayerIndex].Rounds++;
                PlayerList[CurrentPlayerIndex].Marks += CurrentPlayerScores.Sum();

                for (int i = 0; i < Scores[CurrentPlayerIndex].Count; i++)
                {
                    Scores[CurrentPlayerIndex][i] += CurrentPlayerScores[i];

                    if (Scores[CurrentPlayerIndex][i] > 3 && DisabledSectors.Contains(i) == false)
                    {
                        int sector = Sectors[i] == "Bull" ? 25 : int.Parse(Sectors[i]);

                        PlayerList[CurrentPlayerIndex].CurrentPoints += (Scores[CurrentPlayerIndex][i] - 3) * sector;
                        Scores[CurrentPlayerIndex][i] = 3;
                    }
                }

                bool legEndet = HasLegEndet();

                if (legEndet)
                {
                    await Task.Delay(500);
                    await LegEnded();
                }

                else
                    CurrentPlayerIndex = CurrentPlayerIndex == PlayerList.Count - 1 ? 0 : CurrentPlayerIndex + 1;

                CurrentPlayerScores = [0, 0, 0, 0, 0, 0, 0];
                RecentScores = new List<int>();

                //To notify usercontrol about change
                CurrentPlayerScores = CurrentPlayerScores.ToArray().ToList();
                Scores = Scores.ToArray().ToList();
                OnPropertyChanged(nameof(PlayerList));
                OnPropertyChanged(nameof(Scores));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        [RelayCommand]
        private async Task ScoreRemoved()
        {
            if (RecentScores.Any() != true)
                return;

            var indexToRemove = RecentScores[RecentScores.Count - 1];

            CurrentPlayerScores[indexToRemove]--;
            RecentScores.RemoveAt(RecentScores.Count - 1);
            CheckDisabledSectors();

            //To notify usercontrol about change
            CurrentPlayerScores = CurrentPlayerScores.ToArray().ToList();
        }

        internal async Task LegEnded()
        {
            PlayerList[CurrentPlayerIndex].Legs++;
            LegCount++;

            bool setEnded = false;

            await Task.Delay(500);

            if (PlayerList[CurrentPlayerIndex].Legs >= GameParameters.LegCount && PlayerList[CurrentPlayerIndex].Sets + 1 >= GameParameters.SetCount)
            {
                PlayerList[CurrentPlayerIndex].Sets++;

                IsMatchOver = true;
                setEnded = true;
            }

            else if (PlayerList[CurrentPlayerIndex].Legs >= GameParameters.LegCount)
            {
                PlayerList[CurrentPlayerIndex].Sets++;
                setEnded = true;
            }

            await SaveLegResult();

            InitializeNewLeg(setEnded);
        }

        private async Task SaveLegResult()
        {
            try
            {
                for (int i = 0; i < PlayerList.Count; i++)
                {
                    if (PlayerList[i] is CricketDartsBot)
                        continue;

                    var player = PlayerList[i];

                    CricketPlayerDTO cricketPlayerDTO = new CricketPlayerDTO
                    {
                        ID = Guid.NewGuid(),
                        MatchID = MatchID,
                        LegWon = CurrentPlayerIndex == i,
                        Name = player.Name,
                        Marks = player.Marks,
                        Points = player.CurrentPoints,
                        Rounds = player.Rounds,
                        PlayedDateTime = DateTime.Now
                    };


                    _dbDataContext.Add(cricketPlayerDTO);
                    await _dbDataContext.SaveChangesAsync();
                }
                var list = _dbDataContext.CricketLegs.ToList();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(_translator.GetTranslation("Error_Title"), ex.Message + ex.StackTrace, "OK");
            }
        }

        internal override void InitializeNewLeg(bool setEnded)
        {
            Scores = [];

            foreach (var player in PlayerList)
            {
                Scores.Add(new List<int> { 0, 0, 0, 0, 0, 0, 0 });

                player.CurrentPoints = 0;

                if (setEnded)
                    player.Legs = 0;
            }

            if (CurrentPlayerIndex == LegCount % PlayerList.Count)
                CricketGameViewModel_PropertyChanged(null, new System.ComponentModel.PropertyChangedEventArgs(nameof(CurrentPlayerIndex)));

            else
                CurrentPlayerIndex = LegCount % PlayerList.Count;

            DisabledSectors = [];

            OnPropertyChanged(nameof(PlayerList));
            OnPropertyChanged(nameof(Scores));
            OnPropertyChanged(nameof(DisabledSectors));
        }

        private async Task BotThrow()
        {
            try
            {
                var bot = PlayerList[CurrentPlayerIndex] as CricketDartsBot;

                if (bot == null)
                    throw new ArgumentNullException("Current player is not bot");

                await MopupService.Instance.PushAsync(new BotThrowPopupView()
                {
                    BindingContext = this
                });

                await PerformBotThrow();

                await Task.Delay(1000);

                await MopupService.Instance.PopAsync();

                BotTargets = new string[3];
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            finally
            {
                BotThrowBackgrounds = [Colors.Black, Colors.Black, Colors.Black];

                OnPropertyChanged(nameof(BotThrowBackgrounds));
                OnPropertyChanged(nameof(CurrentPlayerName));
            }
        }

        private async Task PerformBotThrow()
        {
            CricketDartsBot? dartsBot = PlayerList[CurrentPlayerIndex] as CricketDartsBot ?? throw new ArgumentNullException("Current player is not cricket bot");

            int score = 0;

            for (int i = 0; i < BotTargets.Length; i++)
            {
                var parameters = GetBotThrowParameters();

                var botTarget = dartsBot.NextTarget(parameters.needsToClose, parameters.canClose, parameters.canScore, parameters.pointDifference, 3 - i);
                BotTargets[i] = ConvertThrowToString(botTarget.sector, botTarget.scoreType);

                OnPropertyChanged(nameof(BotTargets));

                await Task.Delay(2000);


                var botThrow = dartsBot.PerformThrow(BotTargets[i]);

                if (botThrow.sector == -1)
                {
                    BotTargets[i] = "0";
                    botThrow.sector = 0;
                }
                else
                    BotTargets[i] = ConvertThrowToString(botThrow.sector, botThrow.scoreType);

                if (botThrow.sector == botTarget.sector)
                {
                    if (botThrow.scoreType == botTarget.scoreType)
                        BotThrowBackgrounds[i] = Colors.DarkGreen;

                    else
                        BotThrowBackgrounds[i] = Colors.Coral;
                }

                else
                    BotThrowBackgrounds[i] = Colors.Crimson;

                OnPropertyChanged(nameof(BotTargets));
                OnPropertyChanged(nameof(BotThrowBackgrounds));

                int sectorIndex = -1;

                for (int j = 0; j < Sectors.Length; j++)
                {
                    if (Sectors[j] == botThrow.sector.ToString() || botThrow.sector == 25 && j == Sectors.Length - 1)
                    {
                        sectorIndex = j;
                        break;
                    }
                }

                if (sectorIndex != -1)
                {
                    int multiplier = botThrow.scoreType == ScoreType.Triple ? 3 : botThrow.scoreType == ScoreType.Double ? 2 : 1;

                    CurrentPlayerScores[sectorIndex] += 1 * multiplier;

                    CheckDisabledSectors();

                    if (CurrentPlayerScores[sectorIndex] > 3)
                    {
                        if (DisabledSectors.Contains(sectorIndex) == false)
                            PlayerList[CurrentPlayerIndex].CurrentPoints += (CurrentPlayerScores[sectorIndex] - 3) * botThrow.sector;

                        CurrentPlayerScores[sectorIndex] = 3;
                    }
                }

                if (HasLegEndet())
                    break;

                await Task.Delay(1000);
            }

            await ScoreSubmitted();
        }

        private bool HasLegEndet()
        {
            return Scores[CurrentPlayerIndex].Where((x, index) => x + CurrentPlayerScores[index] < 3).Count() == 0
                    && PlayerList[CurrentPlayerIndex].CurrentPoints == PlayerList.
                            OrderByDescending((x) => x.CurrentPoints).First().CurrentPoints;
        }

        private int GetUsedDartsCount(int throwIndex)
        {
            var scores = CurrentPlayerScores.ToArray();

            scores[throwIndex]++;

            int dartsUsed = 0;
            for (int i = 0; i < scores.Length - 1; i++)
            {
                dartsUsed += (scores[i] + 2) / 3;
            }

            dartsUsed += (scores[scores.Length - 1] + 1) / 2;

            return dartsUsed;
        }

        private void CheckDisabledSectors()
        {
            for (int i = 0; i < Scores[0].Count; i++)
            {
                bool isEnabled = false;

                for (int j = 0; j < PlayerList.Count; j++)
                {
                    isEnabled = false;

                    if (Scores[j][i] < 3)
                    {
                        if (j == CurrentPlayerIndex && Scores[j][i] + CurrentPlayerScores[i] >= 3)
                            continue;

                        isEnabled = true;
                        break;
                    }
                }

                if (DisabledSectors.Contains(i) == false && isEnabled == false && CurrentPlayerScores[i] + Scores[CurrentPlayerIndex][i] > 2)
                {
                    DisabledSectors.Add(i);
                }

                else if (DisabledSectors.Contains(i) && (isEnabled || CurrentPlayerScores[i] + Scores[CurrentPlayerIndex][i] < 3))
                    DisabledSectors.Remove(i);
            }

            DisabledSectors = DisabledSectors.ToArray().ToList();
        }

        private (Dictionary<int, int> needsToClose, Dictionary<int, int> canClose, List<int> canScore, int pointDifference) GetBotThrowParameters()
        {
            Dictionary<int, int> needsToClose = [];
            Dictionary<int, int> canClose = [];
            List<int> canScore = [];
            int scoreDifference = int.MinValue;

            for (int i = 0; i < Sectors.Length - 1; i++)
            {
                bool shouldClose = true;
                bool scoreable = false;

                for (int j = 0; j < PlayerList.Count; j++)
                {
                    if (j == CurrentPlayerIndex)
                        continue;

                    if (Scores[j][i] < 3)
                    {
                        scoreable = true;
                        shouldClose = false;
                    }

                    //Gets the score difference to the best player, who is not the current bot
                    if (scoreDifference == int.MinValue)
                        scoreDifference = PlayerList[CurrentPlayerIndex].CurrentPoints - PlayerList.
                            Where((value, index) => index != CurrentPlayerIndex).
                            OrderByDescending((x) => x.CurrentPoints).First().CurrentPoints;
                }

                if (int.TryParse(Sectors[i], out int sector) == false)
                    break;

                if (Scores[CurrentPlayerIndex][i] + CurrentPlayerScores[i] > 2)
                {
                    if (scoreable && DisabledSectors.Contains(i) == false)
                        canScore.Add(sector);
                }

                else
                {
                    int dartsNeeded = 3 - Scores[CurrentPlayerIndex][i];

                    if (shouldClose)
                        needsToClose.Add(sector, dartsNeeded);

                    else
                        canClose.Add(sector, dartsNeeded);
                }
            }

            return (needsToClose, canClose, canScore, scoreDifference);
        }
    }
}
