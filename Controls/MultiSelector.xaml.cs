
using Microsoft.Maui.Controls.Shapes;

namespace DartsApp.Controls;

public partial class MultiSelector : ContentView
{
	public MultiSelector()
	{
		InitializeComponent();
	}

    public int _defaultValue;
    public int DefaultValue
    {
        get { return _defaultValue; }
        set { _defaultValue = value; }
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

    public static BindableProperty SelectedValueProperty = BindableProperty.Create(
        nameof(SelectedValue),
        typeof(int),
        typeof(View),
        default(int),
        BindingMode.TwoWay,
        propertyChanged: OnSelectionChanged);

    public int SelectedValue
	{
		get => (int)GetValue(SelectedValueProperty);
		set => SetValue(SelectedValueProperty, value);
	}

    private static void ValuesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MultiSelector selector && newValue is IEnumerable<string> values && values != null)
        {
            Application.Current.Resources.TryGetValue("SelectedOption", out var SelectedOptionStyle);
            Application.Current.Resources.TryGetValue("UnselectedOption", out var UnselectedOptionStyle);

            for (int i = 0; i < values.Count(); i++)
            {
                var button = new Button();
                button.Text = values.ElementAt(i).Length < 15 ? values.ElementAt(i) : $"{values.ElementAt(i).Substring(0,12)}...";
                button.HeightRequest = selector.HeightRequest;
                button.Style = i == selector.DefaultValue ? SelectedOptionStyle as Style : UnselectedOptionStyle as Style;
                button.FontSize = values.Count() < 3 ? 20 : 12;

                selector.container.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                selector.container.Add(button);
                selector.container.SetColumn(button, i);

                button.Clicked += (s, e) =>
                {
                    selector.SelectedValue = selector.container.GetColumn(button);
                };
            }

            selector.SelectedValue = selector.DefaultValue;
        }
    }

    private static void OnSelectionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MultiSelector selector && newValue is int newIndex && oldValue is int oldIndex)
        {
            Application.Current.Resources.TryGetValue("SelectedOption", out var SelectedOptionStyle);
            Application.Current.Resources.TryGetValue("UnselectedOption", out var UnselectedOptionStyle);

            var children = selector.container.Children;

            Button oldButton = children.ElementAt(oldIndex) as Button;
            Button newButton = children.ElementAt(newIndex) as Button;

            oldButton.Style = UnselectedOptionStyle as Style;
            newButton.Style = SelectedOptionStyle as Style;
        }            
    }
}