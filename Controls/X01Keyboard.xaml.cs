using System.Windows.Input;

namespace DartsApp.Controls;

public partial class X01Keyboard : ContentView
{
	public X01Keyboard()
	{
		InitializeComponent();
	}

	public static BindableProperty TypeScoreCommandProperty = BindableProperty.Create(
		nameof(SubmitScoreCommand),
		typeof(ICommand),
		typeof(X01Keyboard),
		null);

	public ICommand TypeScoreCommand
	{
		get => (ICommand)GetValue(SubmitScoreCommandProperty);
		set => SetValue(SubmitScoreCommandProperty, value);
	}

    public static BindableProperty SubmitScoreCommandProperty = BindableProperty.Create(
        nameof(SubmitScoreCommand),
        typeof(ICommand),
        typeof(X01Keyboard),
        null);

    public ICommand SubmitScoreCommand
    {
        get => (ICommand)GetValue(SubmitScoreCommandProperty);
        set => SetValue(SubmitScoreCommandProperty, value);
    }

    public static BindableProperty RemoveScoreCommandProperty = BindableProperty.Create(
        nameof(RemoveScoreCommand),
        typeof(ICommand),
        typeof(X01Keyboard),
        null);

    public ICommand RemoveScoreCommand
    {
        get => (ICommand)GetValue(RemoveScoreCommandProperty);
        set => SetValue(RemoveScoreCommandProperty, value);
    }
}