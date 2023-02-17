using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    [ContentProperty("Text")]
    public class SingleLineListSubheader : ContentView
    {
        readonly Grid grid;
        readonly Label textLabel;
        readonly BoxView seperator;
        Button2 hiddenButton;

        public static BindableProperty ShowSeperatorProperty =
            BindableProperty.Create(nameof(ShowSeperator), typeof(bool), typeof(SingleLineListSubheader), defaultValue: true, propertyChanged: OnShowSeperatorPropertyChanged);
        static void OnShowSeperatorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
            => ((SingleLineListSubheader)bindable).OnShowSeperatorPropertyChanged();
        public bool ShowSeperator
        {
            get => (bool)GetValue(ShowSeperatorProperty);
            set => SetValue(ShowSeperatorProperty, value);
        }

        public static BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(SingleLineListSubheader), propertyChanged: OnTextPropertyChanged);
        static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
            => ((SingleLineListSubheader)bindable).OnTextPropertyChanged();
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SingleLineListSubheader), propertyChanged: OnCommandPropertyChanged);
        static void OnCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
            => ((SingleLineListSubheader)bindable).OnCommandPropertyChanged();
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SingleLineListSubheader), propertyChanged: OnCommandParameterPropertyChanged);
        static void OnCommandParameterPropertyChanged(BindableObject bindable, object oldValue, object newValue)
            => ((SingleLineListSubheader)bindable).OnCommandParameterPropertyChanged();
        public object CommandParameter
        {
            get => (object)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public SingleLineListSubheader()
        {
            HeightRequest = (double)Forms.Style.DynamicResourceProvider.Instance["size_500"];
            CompressedLayout.SetIsHeadless(this, true);

            grid = new Grid()
            {
                RowSpacing = 0,
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition() { Height = GridLength.Star },
                    new RowDefinition() { Height = new GridLength((double)Forms.Style.DynamicResourceProvider.Instance["size_10"]) }
                }
            };
            CompressedLayout.SetIsHeadless(grid, true);

            textLabel = new Label()
            {
                Style = (Xamarin.Forms.Style)Forms.Style.DynamicResourceProvider.Instance["ListGroupHeaderLabelStyle"],
                InputTransparent = true,
                Text = Text
            };
            Grid.SetRow(textLabel, 0);
            Grid.SetRowSpan(textLabel, 2);
            grid.Children.Add(textLabel);

            seperator = new BoxView()
            {
                Style = (Xamarin.Forms.Style)Forms.Style.DynamicResourceProvider.Instance["DividerStyle"],
                InputTransparent = true
            };
            Grid.SetRow(seperator, 1);
            grid.Children.Add(seperator);

            Content = grid;
        }

        void OnTextPropertyChanged()
        {
            textLabel.Text = Text;
        }

        void OnShowSeperatorPropertyChanged()
        {
            if (ShowSeperator)
            {
                grid.Children.Add(seperator);
                Grid.SetRow(seperator, 1);
            }
            else
            {
                grid.Children.Remove(seperator);
            }
        }

        void CreateHiddenButton()
        {
            hiddenButton = new Button2()
            {
                Style = (Xamarin.Forms.Style)Forms.Style.DynamicResourceProvider.Instance["HiddenButtonStyle"]
            };
            Grid.SetRow(hiddenButton, 0);
            Grid.SetRowSpan(hiddenButton, 2);
        }

        void OnCommandPropertyChanged()
        {
            if (Command != null)
            {
                if (hiddenButton == null)
                {
                    CreateHiddenButton();
                }

                hiddenButton.Command = Command;
                grid.Children.Insert(0, hiddenButton);
            }
            else
            {
                grid.Children.Remove(hiddenButton);
            }
        }

        void OnCommandParameterPropertyChanged()
        {
            if (hiddenButton == null)
            {
                CreateHiddenButton();
            }

            hiddenButton.CommandParameter = CommandParameter;
        }
    }
}


