
using banditoth.MAUI.Multilanguage.Interfaces;
using DartsApp.Converters;
using DartsApp.Entities;

namespace DartsApp.Controls;

public partial class CricketScoreboard : ContentView
{
    public CricketScoreboard()
	{
		InitializeComponent();
	}

    public static BindableProperty SectorsProperty = BindableProperty.Create(
        nameof(Sectors),
        typeof(IEnumerable<string>),
        typeof(CricketScoreboard),
        null,
        propertyChanged: SectorsChanged);

    public IEnumerable<string> Sectors
    {
        get => (IEnumerable<string>)GetValue(SectorsProperty);
        set => SetValue(SectorsProperty, value);
    }

    public static BindableProperty PlayersProperty = BindableProperty.Create(
        nameof(Players),
        typeof(IEnumerable<CricketDartsPlayer>),
        typeof(X01ScoreBoard),
        null,
        propertyChanged: null);

    public IEnumerable<CricketDartsPlayer> Players
    {
        get => (IEnumerable<CricketDartsPlayer>)GetValue(PlayersProperty);
        set => SetValue(PlayersProperty, value);
    }

    private static void SectorsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CricketScoreboard scoreBoard && newValue is IEnumerable<string> sectors)
        {
            ITranslator _translator = IPlatformApplication.Current?.Services.GetService<ITranslator>();

            scoreBoard.container.RowDefinitions.Add(new RowDefinition(40));

            scoreBoard.container.ColumnDefinitions = new ColumnDefinitionCollection(
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto));

            AddGridBackgrounds(scoreBoard);

            var playerLabel = CreateHeaderLabel(_translator?.GetTranslation("Player"));

            playerLabel.HorizontalTextAlignment = TextAlignment.Start;
            playerLabel.HorizontalOptions = LayoutOptions.Start;

            AddViewToGrid(scoreBoard.container, playerLabel, 0, 0);

            for (int i = 0; i < scoreBoard.Sectors.Count(); i++)
            {
                var label = CreateHeaderLabel(scoreBoard.Sectors.ElementAt(i));
                AddViewToGrid(scoreBoard.container, label, 0, i + 1);
            }

            var pointsTitleLabel = CreateHeaderLabel(_translator?.GetTranslation("SelectGameView_Points"));

            AddViewToGrid(scoreBoard.container, pointsTitleLabel, 0, scoreBoard.Sectors.Count() + 1);

            for (int i = 0; i < scoreBoard.Players.Count(); i++)
            {
                scoreBoard.container.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

                var currentPlayer = scoreBoard.Players.ElementAt(i);

                var nameLabel = CreateContentLabel(currentPlayer.Name);

                nameLabel.HorizontalTextAlignment = TextAlignment.Start;
                nameLabel.HorizontalOptions = LayoutOptions.Start;
                nameLabel.Padding = 5;

                AddViewToGrid(scoreBoard.container, nameLabel, i + 1, 0);

                for (int j = 0; j < scoreBoard.Sectors.Count(); j++)
                {
                    var label = CreateContentLabel("");

                    Binding binding = new Binding
                    {
                        Path = $"Scores[{i}]",
                        Converter = new CricketScoreConverter(),
                        ConverterParameter = j
                    };

                    label.SetBinding(Label.TextProperty, binding);
                    

                    AddViewToGrid(scoreBoard.container, label, i + 1, j + 1);
                }

                var pointsLabel = CreateContentLabel(currentPlayer.CurrentPoints.ToString());
                pointsLabel.SetBinding(Label.TextProperty, $"PlayerList[{i}].CurrentPoints");
                AddViewToGrid(scoreBoard.container, pointsLabel, i + 1, scoreBoard.Sectors.Count() + 1);
            }
        }
    }

    private static Label CreateHeaderLabel(string text)
    {
        return new Label()
        {
            Text = text,
            FontSize = 14,
            Padding = new Thickness(5, 0),
            TextColor = Colors.Black,
            FontFamily = "OpenSans-Semibold",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
    }

    private static Label CreateContentLabel(string text)
    {
        return new Label()
        {
            Text = text,
            TextColor = Colors.White,
            FontSize = 14,
            FontFamily = "OpenSans-Semibold",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };
    }

    private static void AddViewToGrid(Grid grid, View view, int row, int column)
    {
        grid.SetRow(view, row);
        grid.SetColumn(view, column);
        grid.Children.Add(view);
    }

    private static void AddGridBackgrounds(CricketScoreboard scoreBoard)
    {
        BoxView firstRowBackground = new BoxView
        {
            Background = Color.FromArgb("#D9D9D9"),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            ZIndex = -1
        };
        AddViewToGrid(scoreBoard.container, firstRowBackground, 0, 0);
        scoreBoard.container.SetColumnSpan(firstRowBackground, scoreBoard.container.ColumnDefinitions.Count);

        BoxView firstColumnBackGround = new BoxView
        {
            Background = Color.FromArgb("#135102"),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            ZIndex = -1
        };
        AddViewToGrid(scoreBoard.container, firstColumnBackGround, 1, 0);
        scoreBoard.container.SetRowSpan(firstColumnBackGround, scoreBoard.Players.Count());

        BoxView otherColumnBackGround = new BoxView
        {
            Background = Colors.Black,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            ZIndex = -1
        };
        AddViewToGrid(scoreBoard.container, otherColumnBackGround, 1, 1);
        scoreBoard.container.SetRowSpan(otherColumnBackGround, scoreBoard.Players.Count());
        scoreBoard.container.SetColumnSpan(otherColumnBackGround, scoreBoard.container.ColumnDefinitions.Count - 1);
    }
}