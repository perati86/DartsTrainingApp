namespace DartsApp.Controls;

public partial class TargetPracticeKeyboard : ContentView
{
	public TargetPracticeKeyboard()
	{
		InitializeComponent();
	}

    public static BindableProperty OptionsProperty = BindableProperty.Create(
       nameof(Options),
       typeof(IEnumerable<string>),
       typeof(TargetPracticeKeyboard),
       null,
       propertyChanged: null);

    public IEnumerable<string> Options
    {
        get => (IEnumerable<string>)GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    //TODO: Change button color when clicked
}