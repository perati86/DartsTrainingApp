using banditoth.MAUI.Multilanguage.Interfaces;
using banditoth.MAUI.MVVM.Interfaces;
using CommunityToolkit.Maui.Media;
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
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public partial class X01GameViewModel : GameViewModel
    {
        private readonly DbDataContext _dbDataContext;
        private readonly ITranslator _translator;

        [ObservableProperty]
        private Guid _matchID;

        [ObservableProperty]
        private List<X01DartsPlayer> _playerList = [];

        [ObservableProperty]
        internal new X01Game _gameParameters;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentPlayerName))]
        private string _currentScore = "0";

        [ObservableProperty]
        private bool _isBotThrowing;

        [ObservableProperty]
        private string[] _botTargets = new string[3];

        [ObservableProperty]
        private Color[] _botThrowBackgrounds = [Colors.Black, Colors.Black, Colors.Black];

        [ObservableProperty]
        private bool _isAskingCheckoutDarts;

        [ObservableProperty]
        private bool _isMatchOver;

        public string CurrentPlayerName => PlayerList?.Any() == true ? PlayerList[CurrentPlayerIndex].Name : "";

        public override void OnViewDisapearing()
        {
            IsMatchOver = true;

            base.OnViewDisapearing();
        }

        public X01GameViewModel(DbDataContext dbDataContext, ITranslator translator)
        {
            _dbDataContext = dbDataContext;
            _translator = translator;

            PropertyChanged += X01GameViewModel_PropertyChanged;
        }

        private async void X01GameViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentPlayerIndex) && PlayerList[CurrentPlayerIndex] is X01DartsBot && IsMatchOver == false)
            {
                if (MopupService.Instance.PopupStack.Count > 0)
                {
                    await Task.Delay(2000);

                    await MopupService.Instance.PopAllAsync();
                }
                await Task.Delay(500);

                await BotThrow();
            }
        }

        public void Initialize(X01Game game, List<X01DartsPlayer> players)
        {
            GameParameters = game;
            PlayerList = players;
            MatchID = Guid.NewGuid();
            CurrentScore = "";

            if (PlayerList[0] is X01DartsBot)
                _ = BotThrow();
        }

        [RelayCommand]
        private async Task ScoreTyped(string score)
        {
            if (int.TryParse(score, out int digit) == false)
                return;

            if (CurrentScore == "0")
            {
                CurrentScore = score;
                return;
            }

            if (CurrentScore.Length > 2)
            {
                CurrentScore = CurrentScore.Substring(0, 2) + score.ToString();
                return;
            }

            CurrentScore += score.ToString();
        }

        [RelayCommand]
        private async Task ScoreSubmitted()
        {
            if (int.TryParse(CurrentScore, out int score) == false)
                return;

            if (IsScoreValid() == false)
            {
                await Application.Current.MainPage.DisplayAlert(_translator.GetTranslation("X01GameViewModel_WrongScorePopup_Title"),
                    _translator.GetTranslation("X01GameViewModel_WrongScorePopup_Text"), "OK");

                CurrentScore = "0";
                return;
            }

            await RegisterScore(score);
        }

        [RelayCommand]
        private async Task ScoreRemoved()
        {
            if (CurrentScore?.Any() != true)
                return;

            if (CurrentScore.Length == 1)
            {
                CurrentScore = "0";
                return;
            }

            CurrentScore = CurrentScore.Remove(CurrentScore.Length - 1);
        }

        private async Task RegisterScore(int score, int dartsUsed = 3)
        {
            if (score > PlayerList[CurrentPlayerIndex].CurrentPoints
                || GameParameters.OutType != ThrowType.Straight && score + 1 == PlayerList[CurrentPlayerIndex].CurrentPoints)
            {
                RegisterScoreToStatistics(0, 3);

                ChangePlayer();

                return;
            }

            if (score == PlayerList[CurrentPlayerIndex].CurrentPoints)
            {
                if (IsValidCheckOut())
                {
                    if (PlayerList[CurrentPlayerIndex] is X01HumanPlayer)
                    {
                        int minimumDarts = GetMinimumNeededDarts(score);

                        dartsUsed = minimumDarts == 3 ? 3 : minimumDarts == 2
                            ? int.Parse(await Application.Current.MainPage.DisplayActionSheet(_translator.GetTranslation("X01GameViewModel_DartsUsed"), null, null, ["2", "3"]))
                            : int.Parse(await Application.Current.MainPage.DisplayActionSheet(_translator.GetTranslation("X01GameViewModel_DartsUsed"), null, null, ["1", "2", "3"]));
                    }

                    PlayerList[CurrentPlayerIndex].Checkout = score;
                    RegisterScoreToStatistics(score, dartsUsed, Math.Min(dartsUsed, await GetCheckoutDarts(score)));

                    await LegEnded();
                    OnPropertyChanged(nameof(PlayerList));

                    return;
                }

                await Application.Current.MainPage.DisplayAlert(_translator.GetTranslation("X01GameViewModel_WrongScorePopup_Title"),
                    _translator.GetTranslation("X01GameViewModel_WrongScorePopup_Text"), "OK");

                return;
            }

            int checkoutDarts = PlayerList[CurrentPlayerIndex] is X01DartsBot ? dartsUsed : await GetCheckoutDarts(score);

            RegisterScoreToStatistics(score, dartsUsed, checkoutDarts);

            ChangePlayer();
        }

        private void RegisterScoreToStatistics(int score, int dartsUsed = 3, int checkoutDartsUsed = 0)
        {
            var player = PlayerList[CurrentPlayerIndex];

            player.CurrentPoints -= score;
            player.TotalPoints += score;
            player.TotalDartsThrown += dartsUsed;
            player.DartsThrownThisLeg += dartsUsed;
            player.CheckoutDartsThrownThisLeg += checkoutDartsUsed;
            player.TotalCheckoutDartsThrown += checkoutDartsUsed;
            
            if (score > player.HighestScoreThisLeg)
            {
                player.HighestScoreThisLeg = score;
                player.HighestScoreCountThisLeg = 1;
            }

            else if (score == player.HighestScoreThisLeg)
                player.HighestScoreCountThisLeg++;

            if (player.DartsThrownThisLeg < 10)
                player.First9DartsPoints += score;

            if (score == 180)
                player.HighThrowCounts[3]++;

            else if (score >= 140)
                player.HighThrowCounts[2]++;

            else if (score >= 100)
                player.HighThrowCounts[1]++;

            else if (score >= 60)
                player.HighThrowCounts[0]++;

            PlayerList[CurrentPlayerIndex] = player;
        }

        private void ChangePlayer()
        {
            CurrentPlayerIndex = CurrentPlayerIndex == PlayerList.Count - 1 ? 0 : CurrentPlayerIndex + 1;
            CurrentScore = "0";

            OnPropertyChanged(nameof(PlayerList));
        }

        private async Task<int> GetCheckoutDarts(int score)
        {
            if (IsAskingCheckoutDarts == false)
                return GetMaximumCheckoutDarts(PlayerList[CurrentPlayerIndex].CurrentPoints, score);

            List<string> checkoutDartsOptions = ["0", "1", "2", "3"];

            var maximumCheckOutDarts = GetMaximumCheckoutDarts(PlayerList[CurrentPlayerIndex].CurrentPoints, score);

            checkoutDartsOptions = checkoutDartsOptions.Where(x => int.Parse(x) <= maximumCheckOutDarts).ToList();

            if (checkoutDartsOptions.Count < 2)
                return 0;

            return int.Parse(await Application.Current.MainPage.DisplayActionSheet(_translator.GetTranslation("X01GameViewModel_CheckoutDartsUsed"), null, null, checkoutDartsOptions.ToArray()) ?? "0");
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
                    if (PlayerList[i] is not X01HumanPlayer humanPlayer)
                        continue;

                    X01PlayerDTO humanPlayerDTO = new X01PlayerDTO
                    {
                        ID = Guid.NewGuid(),
                        MatchID = MatchID,
                        Name = humanPlayer.Name,
                        DartsThrown = humanPlayer.DartsThrownThisLeg,
                        CheckOutDartsThrown = humanPlayer.CheckoutDartsThrownThisLeg,
                        Points = GameParameters.Points - humanPlayer.CurrentPoints,
                        LegWon = i == CurrentPlayerIndex,
                        PlayedDateTime = DateTime.Now,
                        Checkout = humanPlayer.Checkout,
                        First9DartsPoints = humanPlayer.First9DartsPoints,
                        HighestScoreThisLeg = humanPlayer.HighestScoreThisLeg,
                        HighestScoreCountThisLeg = humanPlayer.HighestScoreCountThisLeg,
                        HighThrowCounts = humanPlayer.HighThrowCounts,
                    };

                    
                    _dbDataContext.Add(humanPlayerDTO);
                    await _dbDataContext.SaveChangesAsync();
                }
                var list = _dbDataContext.X01Legs.ToList();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(_translator.GetTranslation("Error_Title"), ex.Message + ex.StackTrace, "OK");
            }
        }

        private bool IsScoreValid()
        {
            if (int.TryParse(CurrentScore, out int score) == false)
                return false;

            if (score == 1 && PlayerList[CurrentPlayerIndex].CurrentPoints == GameParameters.Points && GameParameters.InType != ThrowType.Straight)
                return false;

            if (score > PlayerList[CurrentPlayerIndex].CurrentPoints || score + 1 == PlayerList[CurrentPlayerIndex].CurrentPoints && GameParameters.OutType != ThrowType.Straight)
                return false;

            if (score > 180 || score < 0)
                return false;

            if (score > 172 && score % 3 != 0)
                return false;

            if (score > 162 && score % 3 == 1)
                return false;

            return true;
        }

        private bool IsValidCheckOut()
        {
            if (GameParameters.OutType != ThrowType.Double)
                return true;

            var score = PlayerList[CurrentPlayerIndex].CurrentPoints;

            if (score > 170 || score > 158 && score % 3 == 0 || score > 162 && score % 3 == 1)
                return false;

            return true;
        }


        internal override void InitializeNewLeg(bool setEnded)
        {
            foreach (var player in PlayerList)
            {
                player.CurrentPoints = GameParameters.Points;
                player.DartsThrownThisLeg = 0;
                player.CheckoutDartsThrownThisLeg = 0;
                player.HighThrowCounts = [0, 0, 0, 0];
                player.Checkout = 0;
                player.First9DartsPoints = 0;
                player.HighestScoreThisLeg = 0;
                player.HighestScoreCountThisLeg = 0;

                if (setEnded)
                    player.Legs = 0;
            }

            CurrentScore = "0";

            if (CurrentPlayerIndex == LegCount % PlayerList.Count)
                X01GameViewModel_PropertyChanged(null, new System.ComponentModel.PropertyChangedEventArgs(nameof(CurrentPlayerIndex)));

            else
                CurrentPlayerIndex = LegCount % PlayerList.Count;

            OnPropertyChanged(nameof(PlayerList));
        }

        private int GetMinimumNeededDarts(int score)
        {
            if (GameParameters.OutType == ThrowType.Double)
            {
                if (score < 41 && score % 2 == 0 || score == 50)
                    return 1;

                if (score < 99 || score < 111 && score % 3 == 2 || score == 100)
                    return 2;

                return 3;
            }

            if (GameParameters.OutType == ThrowType.Master)
            {
                if (score < 41 && score % 2 == 0 || score < 61 && score % 3 == 0 || score == 50)
                    return 1;
            }
            else
            {
                if (score < 21 || score < 41 && score % 2 == 0 || score < 61 && score % 3 == 0 || score == 25 || score == 50)
                    return 1;
            }

            if (score < 103 || score < 121 && score % 3 != 1 && score != 113 && score != 116 & score != 118)
                return 2;

            return 3;
        }

        private int GetMaximumCheckoutDarts(int previousScore, int score)
        {
            if (previousScore - score > 50)
                return 0;

            List<string> lines = new List<string>();

            string checkoutFilePath = GameParameters.OutType == ThrowType.Straight ? "checkouts_straight.txt" : GameParameters.OutType == ThrowType.Double ? "checkouts_double.txt" : "checkouts_master.txt";

            Stream fileStream = FileSystem.OpenAppPackageFileAsync(checkoutFilePath).GetAwaiter().GetResult();

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            List<string[]> checkouts = [];

            for (int i = 0; i < lines.Count; i++)
            {
                var row = lines[i].Split(' ');

                if (row[0] == previousScore.ToString())
                    checkouts.Add(row);
            }

            if (checkouts.Count == 0)
                return 0;

            return 3 - (checkouts.OrderBy(x => x.Length).First().Length - 2);
        }

        private async Task BotThrow()
        {
            try
            {
                IsBotThrowing = true;

                var bot = PlayerList[CurrentPlayerIndex] as X01DartsBot;

                if (bot == null)
                    throw new ArgumentNullException("Current player is not bot");

                await MopupService.Instance.PushAsync(new BotThrowPopupView()
                {
                    BindingContext = this
                });

                await PerformBotThrow();

                await Task.Delay(1000);

                await MopupService.Instance.PopAllAsync();

                BotTargets = new string[3];
            }

            catch (Exception ex)
            {
                await MopupService.Instance.PopAllAsync();

                Debug.WriteLine(ex.Message + ex.StackTrace);
            }

            finally
            {
                IsBotThrowing = false;
                BotThrowBackgrounds = [Colors.Black, Colors.Black, Colors.Black];

                OnPropertyChanged(nameof(BotThrowBackgrounds));
                OnPropertyChanged(nameof(CurrentPlayerName));
            }

        }

        private async Task PerformBotThrow()
        {
            X01DartsBot? dartsBot = PlayerList[CurrentPlayerIndex] as X01DartsBot ?? throw new ArgumentNullException("Current player is not bot");

            int score = 0;

            for (int i = 0; i < BotTargets.Length; i++)
            {
                var botTarget = dartsBot.NextTarget(dartsBot.CurrentPoints - score, GameParameters.InType, GameParameters.OutType, GameParameters.Points, 3 - i);
                BotTargets[i] = ConvertThrowToString(botTarget.sector, botTarget.scoreType);

                OnPropertyChanged(nameof(BotTargets));

                await Task.Delay(1500);

                var botThrow = dartsBot.PerformThrow(BotTargets[i]);

                if (botThrow.sector == -1)
                {
                    BotTargets[i] = 0.ToString();
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

                //Check if bot is allowed to start scoring
                if (PlayerList[CurrentPlayerIndex].CurrentPoints - score != GameParameters.Points
                    || GameParameters.InType == ThrowType.Straight
                    || GameParameters.InType == ThrowType.Double && botThrow.scoreType == ScoreType.Double
                    || GameParameters.InType == ThrowType.Master && botThrow.scoreType != ScoreType.Straight)
                {
                    score += botThrow.scoreType == ScoreType.Triple ? botThrow.sector * 3 : botThrow.scoreType == ScoreType.Double ? botThrow.sector * 2 : botThrow.sector;
                }

                if (score >= dartsBot.CurrentPoints || GameParameters.OutType != ThrowType.Straight && score + 1 == dartsBot.CurrentPoints)
                {
                    await RegisterScore(score, i + 1);

                    OnPropertyChanged(nameof(BotTargets));

                    return;
                }

                OnPropertyChanged(nameof(BotTargets));
                OnPropertyChanged(nameof(BotThrowBackgrounds));

                await Task.Delay(1000);
            }

            await RegisterScore(score, 3);
        }
    }
}
