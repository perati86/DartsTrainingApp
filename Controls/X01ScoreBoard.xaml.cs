using banditoth.MAUI.Multilanguage.Interfaces;
using DartsApp.Entities;
using DartsApp.ViewModels;

namespace DartsApp.Controls;

public partial class X01ScoreBoard : ContentView
{
	public X01ScoreBoard()
	{
		InitializeComponent();
	}

    public static BindableProperty PlayersProperty = BindableProperty.Create(
        nameof(Players),
        typeof(IEnumerable<X01DartsPlayer>),
        typeof(X01ScoreBoard),
        null,
        propertyChanged: PlayersChanged);

    public IEnumerable<X01DartsPlayer> Players
    {
        get => (IEnumerable<X01DartsPlayer>)GetValue(PlayersProperty);
        set => SetValue(PlayersProperty, value);
    }

    public static BindableProperty IsDisplayingSetsProperty = BindableProperty.Create(
        nameof(IsDisplayingSets),
        typeof(bool),
        typeof(X01ScoreBoard),
        null,
        BindingMode.TwoWay);

    public bool IsDisplayingSets
    {
        get => (bool)GetValue(IsDisplayingSetsProperty);
        set => SetValue(IsDisplayingSetsProperty, value);
    }

    private static void PlayersChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is X01ScoreBoard scoreBoard && newValue is IEnumerable<DartsPlayer> players)
        {
            ITranslator _translator = IPlatformApplication.Current?.Services.GetService<ITranslator>();

            scoreBoard.container.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            scoreBoard.container.ColumnDefinitions = new ColumnDefinitionCollection(
                new ColumnDefinition(new GridLength(2, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star));

            if (scoreBoard.IsDisplayingSets)
                scoreBoard.container.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            AddGridBackgrounds(scoreBoard);

            var playerLabel = CreateHeaderLabel(_translator?.GetTranslation("Player"));
            var setLabel = CreateHeaderLabel(_translator.GetTranslation("SelectGameView_Sets"));
            var legLabel = CreateHeaderLabel(_translator.GetTranslation("SelectGameView_Legs"));
            var pointLabel = CreateHeaderLabel(_translator.GetTranslation("SelectGameView_Points"));
            var averageTitleLabel = CreateHeaderLabel(_translator.GetTranslation("Average"));

            playerLabel.HorizontalTextAlignment = TextAlignment.Start;
            playerLabel.HorizontalOptions = LayoutOptions.Start;
            playerLabel.Padding = 5;

            AddViewToGrid(scoreBoard.container, playerLabel, 0, 0);
            AddViewToGrid(scoreBoard.container, setLabel, 0, 1);
            AddViewToGrid(scoreBoard.container, legLabel, 0, 2);
            AddViewToGrid(scoreBoard.container, pointLabel, 0, 3);
            AddViewToGrid(scoreBoard.container, averageTitleLabel, 0, 4);

            for (int i = 0; i < scoreBoard.Players.Count(); i++)
            {
                scoreBoard.container.RowDefinitions.Add(new RowDefinition(GridLength.Star));

                var currentPlayer = scoreBoard.Players.ElementAt(i);

                var nameLabel = CreateContentLabel(currentPlayer.Name);
                var setsLabel = CreateContentLabel(currentPlayer.Sets.ToString());
                var legsLabel = CreateContentLabel(currentPlayer.Legs.ToString());
                var pointsLabel = CreateContentLabel(currentPlayer.CurrentPoints.ToString());
                var averageLabel = CreateContentLabel(currentPlayer.Average.ToString());

                nameLabel.HorizontalTextAlignment = TextAlignment.Start;
                nameLabel.HorizontalOptions = LayoutOptions.Start;
                nameLabel.Padding = 5;

                setsLabel.SetBinding(Label.TextProperty, $"PlayerList[{i}].Sets");
                legsLabel.SetBinding(Label.TextProperty, $"PlayerList[{i}].Legs");
                pointsLabel.SetBinding(Label.TextProperty, $"PlayerList[{i}].CurrentPoints");
                averageLabel.SetBinding(Label.TextProperty, $"PlayerList[{i}].Average");

                AddViewToGrid(scoreBoard.container, nameLabel, i + 1, 0);
                AddViewToGrid(scoreBoard.container, setsLabel, i + 1, 1);
                AddViewToGrid(scoreBoard.container, legsLabel, i + 1, 2);
                AddViewToGrid(scoreBoard.container, pointsLabel, i + 1, 3);
                AddViewToGrid(scoreBoard.container, averageLabel, i + 1, 4);
            }
        }
    }

    private static Label CreateHeaderLabel(string text)
    {
        return new Label()
        {
            Text = text,
            FontSize = 14,
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

    private static void AddGridBackgrounds(X01ScoreBoard scoreBoard)
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