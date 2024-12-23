
namespace DartsApp.Controls;

public partial class StatisticsDisplay : ContentView
{
	public StatisticsDisplay()
	{
		InitializeComponent();
	}

    public static BindableProperty TitleProperty = BindableProperty.Create(
       nameof(Title),
       typeof(string),
       typeof(View),
       default(string),
       propertyChanged: null);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static BindableProperty CaptionsProperty = BindableProperty.Create(
        nameof(Captions),
        typeof(IEnumerable<string>),
        typeof(View),
        null,
        propertyChanged: CaptionsChanged);

    public IEnumerable<string> Captions
    {
        get => (IEnumerable<string>)GetValue(CaptionsProperty);
        set => SetValue(CaptionsProperty, value);
    }

    public static BindableProperty ValuesProperty = BindableProperty.Create(
        nameof(Values),
        typeof(IEnumerable<string>),
        typeof(View),
        null,
        propertyChanged: ValuesChanged);

    public IEnumerable<string> Values
    {
        get => (IEnumerable<string>)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    private static void CaptionsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatisticsDisplay display && newValue is IEnumerable<string> captions && captions?.Any() == true)
        {
            display.container.ColumnDefinitions.Clear();

            for (int i = 0; i < captions.Count(); i++)
            {
                display.container.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                var label = CreateLabel(captions.ElementAt(i), 14);

                AddViewToGrid(display.container, label, 0, i * 2);

                if (i < captions.Count() - 1)
                {
                    var boxView = new BoxView()
                    {
                        WidthRequest = 1,
                        VerticalOptions = LayoutOptions.Fill,
                        Background = Colors.Black,
                    };

                    display.container.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

                    AddViewToGrid(display.container, boxView, 0, i * 2 + 1);
                    display.container.SetRowSpan(boxView, 3);
                }
            }
        }
    }

    private static void ValuesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatisticsDisplay display && newValue is IEnumerable<string> values && values?.Any() == true)
        {

            for (int i = 0; i < values.Count(); i++)
            {
                var label = CreateLabel("", 12);
                label.SetBinding(Label.TextProperty, new Binding() { Path = $"Values[{i}]", Source = display});

                AddViewToGrid(display.container, label, 2, i * 2);
            }
        }
    }

    private static Label CreateLabel(string text, int fontSize)
    {
        return new Label()
        {
            Text = text,
            FontSize = fontSize,
            TextColor = Colors.White,
            FontFamily = "OpenSans-Semibold",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
    }

    private static void AddViewToGrid(Grid grid, View view, int row, int column)
    {
        grid.SetRow(view, row);
        grid.SetColumn(view, column);
        grid.Children.Add(view);
    }
}