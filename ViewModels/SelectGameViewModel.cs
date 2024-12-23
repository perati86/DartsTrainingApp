using banditoth.MAUI.Multilanguage.Interfaces;
using banditoth.MAUI.MVVM.Entities;
using banditoth.MAUI.MVVM.Interfaces;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DartsApp.Entities;
using DartsApp.Enumerations;
using DartsApp.Interfaces;
using DartsApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public partial class SelectGameViewModel : BaseViewModel
    {
        private readonly INavigator _navigator;
        private readonly IPersistentStorage _persistentStorage;
        private readonly ITranslator _translator;

        [ObservableProperty]
        private List<string> _gameTypes = ["X01", "Cricket"];

        [ObservableProperty]
        private List<string> _inTypes;

        [ObservableProperty]
        private List<string> _outTypes;

        [ObservableProperty]
        private List<string> _cricketTypes;

        [ObservableProperty]
        private List<string> _pointOptions = ["101", "201", "301", "401", "501", "701", "901", "1201", "1501", "2501"];

        [ObservableProperty]
        private List<string> _cricketPointTypes;

        [ObservableProperty]
        private List<string> setCounts = ["1", "2", "3", "4", "5", "6", "7"];

        [ObservableProperty]
        private List<string> legCounts = ["1", "2", "3", "4", "5", "6", "7"];

        [ObservableProperty]
        private ObservableCollection<DartsPlayer> _playerList = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsX01Selected))]
        private int _selectedGameType;

        [ObservableProperty]
        private int _selectedInType;

        [ObservableProperty]
        private int _selectedOutType;

        [ObservableProperty]
        private int _selectedCricketType;

        [ObservableProperty]
        private string _selectedCricketPointType;

        [ObservableProperty]
        private int _selectedPoints = 501;

        [ObservableProperty]
        private int _selectedLegs = 3;

        [ObservableProperty]
        private int _selectedSets = 1;

        public bool IsX01Selected => SelectedGameType == 0;

        public SelectGameViewModel(INavigator navigator, IPersistentStorage persistentStorage, ITranslator translator)
        {
            _navigator = navigator;
            _persistentStorage = persistentStorage;
            _translator = translator;
        }

        public void Initialize()
        {
            InTypes = [_translator.GetTranslation("StraightIn"), _translator.GetTranslation("DoubleIn"), _translator.GetTranslation("MasterIn")];
            OutTypes = [_translator.GetTranslation("StraightOut"), _translator.GetTranslation("DoubleOut"), _translator.GetTranslation("MasterOut")];
            CricketTypes = [_translator.GetTranslation("CricketType_Classic"), "Cut throat", _translator.GetTranslation("CricketType_RandomNumbers")];
            CricketPointTypes = [_translator.GetTranslation("On"), _translator.GetTranslation("Off")];

            SelectedCricketPointType = CricketPointTypes[0];
            SelectedOutType = 1;
        }

        [RelayCommand]
        private async Task AddPlayer()
        {
            var storedPlayerList = string.IsNullOrEmpty(_persistentStorage.PlayerList) ? []
                : JsonSerializer.Deserialize<string[]>(_persistentStorage.PlayerList);

            var playerName = storedPlayerList.Where(x => PlayerList.Select(x => x.Name).Contains(x) == false).ToArray().Length == 0

                ? await Application.Current.MainPage.DisplayPromptAsync(_translator.GetTranslation("PlayerListViewModel_NewPlayer_Title"),
                    _translator.GetTranslation("PlayerListViewModel_NewPlayer_Text"), 
                    placeholder: _translator.GetTranslation("PlayerListViewModel_NewPlayer_PlaceHolder"))

                : await Application.Current.MainPage.DisplayActionSheet(_translator.GetTranslation("SelectGameViewModel_AddPlayer"),
                _translator.GetTranslation("Cancel"), null, storedPlayerList.Where(x => PlayerList.Select(x => x.Name).Contains(x) == false).ToArray());

            if (string.IsNullOrEmpty(playerName) || playerName == _translator.GetTranslation("Cancel"))
                return;

            PlayerList.Add(new DartsPlayer() { Name = playerName });

            PlayerList = PlayerList;
        }

        //TODO: Add BotList with names, add new Player to entity framework, delete player

        [RelayCommand]
        private async Task AddBot()
        {
            var bot = new X01DartsBot() { Name = "Botond", Level = 10 };

            var level = await Application.Current.MainPage.DisplayPromptAsync("New player", "Add new player", placeholder: "1-20", maxLength:2, keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(level))
                return;

            bot.Level = int.Parse(level);
            bot.Name = $"{bot.Name} (Level {level})";

            PlayerList.Add(bot);

            PlayerList = PlayerList;
        }

        [RelayCommand]
        private async Task RemovePlayer(DartsPlayer player)
        {
            PlayerList.Remove(player);

            PlayerList = PlayerList;
        }

        [RelayCommand]
        private async Task StartGame()
        {
            try
            {
                if (PlayerList.Count < 1)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", _translator.GetTranslation("Error_SelectGameViewModel_AddMorePlayers"), "OK");
                    return;
                }

                if (IsX01Selected)
                {
                    var x01Game = GetX01Game();
                    var x01PlayerList = new List<X01DartsPlayer>();

                    for (int i = 0; i < PlayerList.Count; i++)
                    {
                        if (PlayerList.ElementAt(i) is X01DartsBot bot)
                        {
                            var botToAdd = new X01DartsBot();

                            botToAdd.Name = bot.Name;
                            botToAdd.Level = bot.Level;
                            botToAdd.CurrentPoints = x01Game.Points;

                            x01PlayerList.Add(botToAdd);

                            continue;
                        }
                        
                        X01HumanPlayer player = new X01HumanPlayer();

                        player.Name = PlayerList[i].Name;
                        player.CurrentPoints = x01Game.Points;

                        x01PlayerList.Add(player);
                    };

                    await Navigation.PushAsync(_navigator.GetInstance<X01GameViewModel>((vm, v) =>
                    {
                        vm.Initialize(x01Game, x01PlayerList);
                    }));
                    return;
                }

                var cricketGame = GetCricketGame();
                var cricketPlayerList = new List<CricketDartsPlayer>();

                for (int i = 0; i < PlayerList.Count; i++)
                {
                    if (PlayerList.ElementAt(i) is X01DartsBot dartsbot)
                    {
                        cricketPlayerList.Add(new CricketDartsBot { Level = dartsbot.Level, Name = dartsbot.Name });
                    }
                    else
                    {
                        cricketPlayerList.Add(new CricketDartsPlayer { Name = PlayerList[i].Name });
                    }
                }

                await Navigation.PushAsync(_navigator.GetInstance<CricketGameViewModel>((vm, v) =>
                {
                    vm.Initialize(cricketGame, cricketPlayerList);
                }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private X01Game GetX01Game()
        {
            return new X01Game()
            {
                InType = SelectedInType == 0 ? ThrowType.Straight : SelectedInType == 1 ? ThrowType.Double : ThrowType.Master,
                OutType = SelectedOutType == 0 ? ThrowType.Straight : SelectedOutType == 1 ? ThrowType.Double : ThrowType.Master,
                LegCount = SelectedLegs,
                SetCount = SelectedSets,
                Points = SelectedPoints
            };
        }

        private CricketGame GetCricketGame()
        {
            return new CricketGame()
            {
                Type = SelectedCricketType == 0 ? CricketGameType.Classic : SelectedCricketType == 1 ? CricketGameType.Cut_throat : CricketGameType.Random_numbers,
                LegCount = SelectedLegs,
                SetCount = SelectedSets,
                HasPoints = SelectedCricketPointType == CricketPointTypes[0]
            };
        }
    }
}
