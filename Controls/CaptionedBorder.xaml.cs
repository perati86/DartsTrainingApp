namespace DartsApp.Controls;

public partial class CaptionedBorder : ContentView
{
	public CaptionedBorder()
	{
		InitializeComponent();
	}

    public static BindableProperty CaptionProperty = BindableProperty.Create(
       nameof(Caption),
       typeof(string),
       typeof(View),
       default(string),
       propertyChanged: null);

    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public static BindableProperty ValueProperty = BindableProperty.Create(
       nameof(Value),
       typeof(string),
       typeof(View),
       default(string),
       propertyChanged: null);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}