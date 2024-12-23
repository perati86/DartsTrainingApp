using banditoth.MAUI.MVVM.Entities;
using banditoth.MAUI.MVVM.Interfaces;
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
    public partial class SelectStatisticsViewModel : BaseViewModel
    {
        private readonly INavigator _navigator;

        [ObservableProperty]
        private DartsPlayer _player;

        public SelectStatisticsViewModel(INavigator navigator)
        {
            _navigator = navigator;
        }

        public void Initialize(DartsPlayer player)
        {
            Player = player;
        }

        [RelayCommand]
        private async Task OpenX01Statistics()
        {
            await Navigation.PushAsync(_navigator.GetInstance<X01StatisticsViewModel>((vm, v) => 
            {
                vm.Initialize(new X01HumanPlayer() { Name = Player.Name});
            }));
        }

        [RelayCommand]
        private async Task OpenCricketStatistics()
        {
            await Navigation.PushAsync(_navigator.GetInstance<CricketStatisticsViewModel>((vm, v) =>
            {
                vm.Initialize(new CricketDartsPlayer() { Name = Player.Name});
            }));
        }

        [RelayCommand]
        private async Task OpenScoringStatistics()
        {
            await Navigation.PushAsync(_navigator.GetInstance<ScoringPracticeStatisticsViewModel>((vm, v) =>
            {
                vm.Initialize(Player.Name);
            }));
        }

        [RelayCommand]
        private async Task OpenDoublesStatistics()
        {
            await Navigation.PushAsync(_navigator.GetInstance<DoublesPracticeStatisticsViewModel>((vm, v) =>
            {
                vm.Initialize(Player.Name);
            }));
        }

        //[RelayCommand]
        //private async Task Open121Statistics()
        //{
        //    await Navigation.PushAsync(_navigator.GetInstance<121StatisticsViewModel>((vm, v) =>
        //    {
        //        vm.Initialize(Player as X01HumanPlayer);
        //    }));
        //}
    }
}
