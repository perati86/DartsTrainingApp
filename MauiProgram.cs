using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using banditoth.MAUI.MVVM;
using DartsApp.ViewModels;
using DartsApp.Views;
using Mopups;
using Mopups.Hosting;
using Mopups.Interfaces;
using Mopups.Services;
using DartsApp.Services;
using DartsApp.Interfaces;
using banditoth.MAUI.Multilanguage;
using DartsApp.Resources.Translations;

namespace DartsApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureMopups()
                .ConfigureMultilanguage(config =>
                {
                    // Set the source of the translations
                    // You can use multiple resource managers by calling UseResource multiple times.
                    config.UseResource(DartsAppTranslations.ResourceManager);

                    // If the app is not storing last used culture, this culture will be used by default
                    config.UseDefaultCulture(new System.Globalization.CultureInfo("en-US"));

                    // Determines whether the app should store the last used culture
                    config.StoreLastUsedCulture(true);

                    // Determines whether the app should throw an exception if a translation is not found.
                    config.ThrowExceptionIfTranslationNotFound(false);

                    // You can set custom translation not found text by calling this method 
                    config.SetTranslationNotFoundText("Transl_Not_Found:", appendTranslationKey: true);
                })
                .ConfigureMvvm(config =>
                {
                    config.Register(typeof(MainViewModel), typeof(MainPage));
                    config.Register(typeof(SelectGameViewModel), typeof(SelectGameView));
                    config.Register(typeof(X01GameViewModel), typeof(X01GameView));
                    config.Register(typeof(CricketGameViewModel), typeof(CricketGameView));
                    config.Register(typeof(SelectTrainingViewModel), typeof(SelectTrainingView));
                    config.Register(typeof(ScoringPracticeViewModel), typeof(ScoringPracticeView));
                    config.Register(typeof(Practice121ViewModel), typeof(Practice121View));
                    config.Register(typeof(X01StatisticsViewModel), typeof(X01StatisticsView));
                    config.Register(typeof(CricketStatisticsViewModel), typeof(CricketStatisticsView));
                    config.Register(typeof(ScoringPracticeStatisticsViewModel), typeof(ScoringPracticeStatisticsView));
                    config.Register(typeof(DoublesPracticeStatisticsViewModel), typeof(DoublesPracticeStatisticsView));
                    config.Register(typeof(SelectStatisticsViewModel), typeof(SelectStatisticsView));
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IPopupNavigation>(MopupService.Instance);
            builder.Services.AddSingleton<IPersistentStorage>(new PersistentStorageService());
            builder.Services.AddDbContext<DbDataContext>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
