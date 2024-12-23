using banditoth.MAUI.Multilanguage.Interfaces;
using banditoth.MAUI.MVVM.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DartsApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public partial class Practice121ViewModel : BaseViewModel
    {
        private readonly ITranslator _translator;

        [ObservableProperty]
        private X01HumanPlayer _player;

        [ObservableProperty]
        private int _checkpoint = 121;

        [ObservableProperty]
        private int _checkout = 121;

        [ObservableProperty]
        private string _currentScore = "0";

        [ObservableProperty]
        private int _currentRound = 1;

        public Practice121ViewModel(ITranslator translator)
        {
            _translator = translator;
        }

        public void Initialize(X01HumanPlayer humanPlayer)
        {
            Player = humanPlayer;
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

            if (CurrentScore?.Any() != true || CurrentScore == "0")
            {
                await RegisterScore(0);

                return;
            }

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
            if (score == Player.CurrentPoints)
            {
                if (IsValidCheckOut())
                {
                    int minimumDarts = GetMinimumCheckoutDarts(score);

                    dartsUsed = minimumDarts == 3 ? 3 : minimumDarts == 2
                        ? int.Parse(await Application.Current.MainPage.DisplayActionSheet(_translator.GetTranslation("X01GameViewModel_DartsUsed"), null, null, ["2", "3"]))
                        : int.Parse(await Application.Current.MainPage.DisplayActionSheet(_translator.GetTranslation("X01GameViewModel_DartsUsed"), null, null, ["1", "2", "3"]));

                    RegisterScoreToStatistics(score, dartsUsed);

                    await StartNewLeg(true);
                    OnPropertyChanged(nameof(Player));

                    return;
                }

                score = 0;
            }

            RegisterScoreToStatistics(score, dartsUsed);

            if (CurrentRound > 3)
                await StartNewLeg(false);

            OnPropertyChanged(nameof(Player));
        }

        private void RegisterScoreToStatistics(int score, int dartsUsed = 3)
        {
            Player.CurrentPoints -= score;
            Player.TotalPoints += score;
            Player.TotalDartsThrown += dartsUsed;
            Player.DartsThrownThisLeg += dartsUsed;

            CurrentRound++;
            CurrentScore = "0";
        }

        private async Task StartNewLeg(bool success)
        {
            if ( Player.DartsThrownThisLeg <= 3 && success)
            {
                Checkpoint = Checkout;
                Checkout++;
            }

            else if (success)
            {
                Checkout++;
            }

            else
            {
                Checkout = Checkpoint;
            }

            InitializeNewLeg();
        }

        internal void InitializeNewLeg()
        {
            CurrentRound = 1;

            Player.CurrentPoints = Checkout;
            Player.DartsThrownThisLeg = 0;
            Player.CheckoutDartsThrownThisLeg = 0;

            OnPropertyChanged(nameof(Player));
        }

        private bool IsScoreValid()
        {
            if (int.TryParse(CurrentScore, out int score) == false)
                return false;

            if (score == 1 && Player.CurrentPoints == 1)
                return false;

            if (score > Player.CurrentPoints || score + 1 == Player.CurrentPoints)
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
            var score = Player.CurrentPoints;

            if (score > 170 || score > 158 && score % 3 == 0 || score > 162 && score % 3 == 1)
                return false;

            return true;
        }

        private int GetMinimumCheckoutDarts(int score)
        {
            if (score < 41 && score % 2 == 0 || score == 50)
                return 1;

            if (score < 99 || score < 111 && score % 3 == 2 || score == 100)
                return 2;

            return 3;
        }
    }
}
