using banditoth.MAUI.Multilanguage.Interfaces;

namespace DartsApp.Controls;

public partial class CaptionedSelector : ContentView
{
    ITranslator _translator = IPlatformApplication.Current?.Services.GetService<ITranslator>();

    public CaptionedSelector()
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

    public static BindableProperty SelectedValueProperty = BindableProperty.Create(
       nameof(SelectedValue),
       typeof(object),
       typeof(CaptionedSelector),
       default(object),
       BindingMode.TwoWay,
       propertyChanged: SelectedValueChanged);

    public object SelectedValue
    {
        get => (object)GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public static BindableProperty ValuesProperty = BindableProperty.Create(
       nameof(Values),
       typeof(IEnumerable<string>),
       typeof(CaptionedSelector),
       null,
       propertyChanged: null);

    public IEnumerable<string> Values
    {
        get => (IEnumerable<string>)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    private static void SelectedValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CaptionedSelector selector && newValue is string value && value != null)
        {
            selector.SelectedValue = value;
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string result = await Application.Current.MainPage.DisplayActionSheet(Caption, _translator.GetTranslation("Cancel"), null, Values.ToArray());

        if (result != null && result != _translator.GetTranslation("Cancel"))
        {
            SelectedValue = result;
        }
    }
}