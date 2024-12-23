using System.Windows.Input;

namespace DartsApp.Controls;

public partial class CricketKeyboard : ContentView
{
	public CricketKeyboard()
	{
		InitializeComponent();
	}

    public static BindableProperty TypeScoreCommandProperty = BindableProperty.Create(
        nameof(SubmitScoreCommand),
        typeof(ICommand),
        typeof(CricketKeyboard),
        null);

    public ICommand TypeScoreCommand
    {
        get => (ICommand)GetValue(TypeScoreCommandProperty);
        set => SetValue(TypeScoreCommandProperty, value);
    }

    public static BindableProperty SubmitScoreCommandProperty = BindableProperty.Create(
        nameof(SubmitScoreCommand),
        typeof(ICommand),
        typeof(CricketKeyboard),
        null);

    public ICommand SubmitScoreCommand
    {
        get => (ICommand)GetValue(SubmitScoreCommandProperty);
        set => SetValue(SubmitScoreCommandProperty, value);
    }

    public static BindableProperty RemoveScoreCommandProperty = BindableProperty.Create(
        nameof(RemoveScoreCommand),
        typeof(ICommand),
        typeof(CricketKeyboard),
        null);

    public ICommand RemoveScoreCommand
    {
        get => (ICommand)GetValue(RemoveScoreCommandProperty);
        set => SetValue(RemoveScoreCommandProperty, value);
    }

    public static BindableProperty ScoresProperty = BindableProperty.Create(
        nameof(Scores),
        typeof(IEnumerable<int>),
        typeof(CricketKeyboard),
        null,
        propertyChanged: OnScoresChanged);

    public IEnumerable<int> Scores
    {
        get => (IEnumerable<int>)GetValue(ScoresProperty);
        set => SetValue(ScoresProperty, value);
    }

    public static BindableProperty DisabledSectorsProperty = BindableProperty.Create(
        nameof(DisabledSectors),
        typeof(IEnumerable<int>),
        typeof(CricketKeyboard),
        null,
        propertyChanged: OnDisabledSectorsChanged);

    public IEnumerable<int> DisabledSectors
    {
        get => (IEnumerable<int>)GetValue(DisabledSectorsProperty);
        set => SetValue(DisabledSectorsProperty, value);
    }

    private static void OnScoresChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CricketKeyboard keyboard && newValue is IEnumerable<int> scores && scores.Any() == true)
        {
            List<Button> buttons = keyboard.container.Children.OfType<Button>().Where((x) => x.CommandParameter is not null).ToList();

            for (int i = 0; i < buttons.Count(); i++)
            {
                if (buttons[i].IsEnabled == false)
                    continue;

                if (scores.ElementAt(i) == 0)
                    buttons[i].Background = Application.Current.Resources["Gray900"] as Color;

                if (scores.ElementAt(i) > 0)
                    buttons[i].Background = Colors.Peru;
            }
        }
    }

    private static void OnDisabledSectorsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CricketKeyboard keyboard && newValue is IEnumerable<int> disabledSectors)
        {
            List<Button> buttons = keyboard.container.Children.OfType<Button>().Where((x) => x.CommandParameter is not null).ToList();

            if (disabledSectors.Any() != true)
            {
                buttons.ForEach((x) => x.IsEnabled = true);
                return;
            }

            for (int i = 0; i < buttons.Count(); i++)
            {
                buttons[i].IsEnabled = (disabledSectors.Contains(i)) ? false : true;

                if (buttons[i].IsEnabled == false)
                    buttons[i].Background = Application.Current.Resources["Gray600"] as Color;
            }
        }
    }
}