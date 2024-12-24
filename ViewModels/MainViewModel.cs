using banditoth.MAUI.Multilanguage.Interfaces;
using banditoth.MAUI.MVVM.Entities;
using banditoth.MAUI.MVVM.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DartsApp.Entities;
using DartsApp.Interfaces;
using DartsApp.Services;
using DartsApp.Views;
using Mopups.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly INavigator _navigator;
        private readonly IPersistentStorage _persistentStorage;
        private readonly ITranslator _translator;
        private readonly DbDataContext _dbDataContext;

        [ObservableProperty]
        private string _currentFlagSource;

        public MainViewModel(INavigator navigator, IPersistentStorage persistentStorage, ITranslator translator, DbDataContext dbDataContext)
        {
            _navigator = navigator;
            _persistentStorage = persistentStorage;
            _translator = translator;
            _dbDataContext = dbDataContext;

            CurrentFlagSource = $"flag_{_translator.CurrentCulture.TwoLetterISOLanguageName}";
        }


        [RelayCommand]
        private async Task StartNewGame()
        {
            await Navigation.PushAsync(_navigator.GetInstance<SelectGameViewModel>((vm, v) => 
            {
                vm.Initialize();
            }));
        }

        [RelayCommand]
        private async Task StartTraining()
        {
            var playerList = string.IsNullOrEmpty(_persistentStorage.PlayerList) ? null : JsonSerializer.Deserialize<string[]>(_persistentStorage.PlayerList);

            if (playerList is null)
            {
                await Application.Current.MainPage.DisplayAlert(_translator.GetTranslation("Error_Title"), _translator.GetTranslation("Error_SelectGameViewModel_AddMorePlayers"), "OK");
                return;
            }

            var playerName = await Application.Current.MainPage.DisplayActionSheet(_translator.GetTranslation("MainViewModel_SelectPlayer"),
                _translator.GetTranslation("Cancel"), null, playerList);

            if (string.IsNullOrEmpty(playerName) || playerName == _translator.GetTranslation("Cancel"))
                return;

            await Navigation.PushAsync(_navigator.GetInstance<SelectTrainingViewModel>((vm, v) =>
            {
                vm.Initialize(playerName);
            }));
        }

        [RelayCommand]
        private async Task OpenStatistics()
        {
            var playerList = string.IsNullOrEmpty(_persistentStorage.PlayerList) ? null : JsonSerializer.Deserialize<string[]>(_persistentStorage.PlayerList);

            if (playerList is null)
            {
                await Application.Current.MainPage.DisplayAlert(_translator.GetTranslation("Error_Title"), _translator.GetTranslation("Error_SelectGameViewModel_AddMorePlayers"), "OK");
                return;
            }

            var playerName = await Application.Current.MainPage.DisplayActionSheet(_translator.GetTranslation("MainViewModel_SelectPlayer"),
                _translator.GetTranslation("Cancel"), null, playerList);

            if (string.IsNullOrEmpty(playerName) || playerName == _translator.GetTranslation("Cancel"))
                return;

            await Navigation.PushAsync(_navigator.GetInstance<SelectStatisticsViewModel>((vm, v) => 
            {
                vm.Initialize(new DartsPlayer() { Name = playerName});
            }));
        }

        [RelayCommand]
        private async Task OpenPlayerList()
        {
            var vm = new PlayerListViewModel(_persistentStorage, _translator, _dbDataContext);
            vm.Initialize();

            var popup = new PlayerListView()
            {
                BindingContext = vm
            };

            await MopupService.Instance.PushAsync(popup);
        }

        [RelayCommand]
        private async Task ChangeLanguage()
        {
            var language = await Application.Current.MainPage.DisplayActionSheet(_translator.GetTranslation("ChangeLanguage_Title"), _translator.GetTranslation("Cancel"), null,
                ["Deutsch", "English", "Español", "Français", "Italiano", "Magyar"]);

            if (language == null || language == _translator.GetTranslation("Cancel")) 
                return;

            switch (language)
            {
                case "Deutsch":
                    _translator.SetCurrentCulture(new CultureInfo("de"));
                    break;

                case "Español":
                    _translator.SetCurrentCulture(new CultureInfo("es"));
                    break;

                case "Français":
                    _translator.SetCurrentCulture(new CultureInfo("fr"));
                    break;

                case "Italiano":
                    _translator.SetCurrentCulture(new CultureInfo("it"));
                    break;

                case "Magyar":
                    _translator.SetCurrentCulture(new CultureInfo("hu"));
                    break;

                default:
                    _translator.SetCurrentCulture(new CultureInfo("en-US"));
                    break;
            }

            CurrentFlagSource = $"flag_{_translator.CurrentCulture.TwoLetterISOLanguageName}";
        }
    }
}
