using banditoth.MAUI.MVVM.Entities;
using banditoth.MAUI.MVVM.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DartsApp.Entities;
using DartsApp.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public partial class SelectTrainingViewModel : BaseViewModel
    {
        private readonly INavigator _navigator;

        [ObservableProperty]
        private string _name;

        public SelectTrainingViewModel(INavigator navigator)
        {
            _navigator = navigator;
        }

        public void Initialize(string name)
        {
            Name = name;
        }

        [RelayCommand]
        private async Task StartScoringPractice()
        {
            await Navigation.PushAsync(_navigator.GetInstance<ScoringPracticeViewModel>((vm, v) =>
            {
                vm.Initialize(PracticeType.Scoring, Name);
            }));
        }

        [RelayCommand]
        private async Task StartDoublesPractice()
        {
            await Navigation.PushAsync(_navigator.GetInstance<ScoringPracticeViewModel>((vm, v) =>
            {
                vm.Initialize(PracticeType.Doubles, Name);
            }));
        }

        [RelayCommand]
        private async Task Start121Practice()
        {
            await Navigation.PushAsync(_navigator.GetInstance<Practice121ViewModel>((vm, v) =>
            {
                vm.Initialize(new X01HumanPlayer()
                {
                    CurrentPoints = 121,
                    Name = Name
                });
            }));
        }
    }
}
