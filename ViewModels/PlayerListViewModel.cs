using banditoth.MAUI.Multilanguage.Interfaces;
using banditoth.MAUI.MVVM.Entities;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DartsApp.Entities.DTO;
using DartsApp.Interfaces;
using DartsApp.Services;
using DartsApp.Views;
using Microsoft.EntityFrameworkCore;
using Mopups.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace DartsApp.ViewModels
{
    public partial class PlayerListViewModel : BaseViewModel
    {
        private readonly IPersistentStorage _persistentStorage;
        private readonly ITranslator _translator;
        private readonly DbDataContext _dbDataContext;

        [ObservableProperty]
        private ObservableCollection<string> _players;

        [ObservableProperty]
        private string _selectedPlayer;

        public PlayerListViewModel(IPersistentStorage persistentStorage, ITranslator translator, DbDataContext dbDataContext)
        {
            _persistentStorage = persistentStorage;
            _translator = translator;
            _dbDataContext = dbDataContext;
        }

        public void Initialize()
        {
            Players = string.IsNullOrEmpty(_persistentStorage.PlayerList) ? new ObservableCollection<string>() : JsonSerializer.Deserialize<ObservableCollection<string>>(_persistentStorage.PlayerList);
        }

        [RelayCommand]
        private async Task RemovePlayer()
        {
            if (string.IsNullOrEmpty(SelectedPlayer) || Players.Contains(SelectedPlayer) == false)
                return;

            var isDeletionConfirmed = await MopupService.Instance.PopupStack[MopupService.Instance.PopupStack.Count - 1]
                    .DisplayAlert(_translator.GetTranslation("PlayerListViewModel_RemoveConfirmation_Title"),
                    _translator.GetTranslation("PlayerListViewModel_RemoveConfirmation_Text"), _translator.GetTranslation("Delete"), _translator.GetTranslation("Cancel"));

            if (isDeletionConfirmed == false)
                return;

            Players.Remove(SelectedPlayer);
            SelectedPlayer = Players.Count > 0 ? Players[0] : null;

            _dbDataContext.RemoveRange(_dbDataContext.X01Legs.Where(x => x.Name == SelectedPlayer));
            _dbDataContext.RemoveRange(_dbDataContext.CricketLegs.Where(x => x.Name == SelectedPlayer));
            _dbDataContext.RemoveRange(_dbDataContext.ScoringPracticeLegs.Where(x => x.Name == SelectedPlayer));
            _dbDataContext.RemoveRange(_dbDataContext.DoublesPracticeLegs.Where(x => x.Name == SelectedPlayer));

            await _dbDataContext.SaveChangesAsync();

            UpdateStorage();
        }

        [RelayCommand]
        private async Task AddPlayer()
        {
            try
            {
                var playerName = await MopupService.Instance.PopupStack[MopupService.Instance.PopupStack.Count - 1]
                    .DisplayPromptAsync(_translator.GetTranslation("PlayerListViewModel_NewPlayer_Title"),
                        _translator.GetTranslation("PlayerListViewModel_NewPlayer_Text"),
                        placeholder: _translator.GetTranslation("PlayerListViewModel_NewPlayer_PlaceHolder"));

                if (string.IsNullOrWhiteSpace(playerName))
                    return;

                if (Players.Contains(playerName))
                {
                    await MopupService.Instance.PopupStack[MopupService.Instance.PopupStack.Count - 1]
                        .DisplayAlert(_translator.GetTranslation("PlayerListViewModel_WrongName_Title"),
                        _translator.GetTranslation("PlayerListViewModel_WrongName_Text"), "OK");

                    return;
                }

                Players.Add(playerName);
                SelectedPlayer = playerName;

                UpdateStorage();
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }

        private void UpdateStorage()
        {
            _persistentStorage.PlayerList = JsonSerializer.Serialize(Players);

            Players = Players.ToArray().ToObservableCollection();
        }
    }
}
