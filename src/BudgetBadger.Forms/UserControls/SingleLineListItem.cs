using System;
using Xamarin.Forms;
using static Dropbox.Api.Files.FileCategory;

namespace BudgetBadger.Forms.UserControls
{
	public class SingleLineListItem : ContentView
	{
        readonly Grid grid;
        readonly Label textLabel;
        Lazy<Label> rightTextLabel;

        public static BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(SingleLineListItem), propertyChanged: OnTextPropertyChanged);
        static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
            => ((SingleLineListItem)bindable).OnTextPropertyChanged();
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty RightTextProperty =
            BindableProperty.Create(nameof(RightText), typeof(string), typeof(SingleLineListItem), propertyChanged: OnRightTextPropertyChanged);
        static void OnRightTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
            => ((SingleLineListItem)bindable).OnRightTextPropertyChanged();
        public string RightText
        {
            get => (string)GetValue(RightTextProperty);
            set => SetValue(RightTextProperty, value);
        }

        public SingleLineListItem()
        {
            HeightRequest = (double)Forms.Style.DynamicResourceProvider.Instance["size_500"];
            CompressedLayout.SetIsHeadless(this, true);

            grid = new Grid()
            {
                RowSpacing = 0,
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition() { Width = GridLength.Star }
                }
            };
            CompressedLayout.SetIsHeadless(grid, true);

            textLabel = new Label()
            {
                Style = (Xamarin.Forms.Style)Forms.Style.DynamicResourceProvider.Instance["ListSingleLineItemLabelStyle"],
                Text = Text
            };
            Grid.SetColumnSpan(textLabel, 2);
            grid.Children.Add(textLabel);

            rightTextLabel = new Lazy<Label>(()
                => new Label()
                {
                    Style = (Xamarin.Forms.Style)Forms.Style.DynamicResourceProvider.Instance["ListSingleLineItemLabelStyle"],
                    HorizontalOptions = LayoutOptions.End,
                    LineBreakMode = LineBreakMode.HeadTruncation,
                    Text = RightText
                });

            Content = grid;
        }

        void OnTextPropertyChanged()
        {
            textLabel.Text = Text;
        }

        void OnRightTextPropertyChanged()
        {
            rightTextLabel.Value.Text = RightText;
            if (string.IsNullOrEmpty(RightText))
            {
                Grid.SetColumnSpan(textLabel, 2);
                if (grid.Children.Contains(rightTextLabel.Value))
                {
                    grid.Children.Remove(rightTextLabel.Value);
                }
            }
            else
            {
                Grid.SetColumnSpan(textLabel, 1);
                Grid.SetColumn(rightTextLabel.Value, 1);
                grid.Children.Add(rightTextLabel.Value);
            }
        }
    }
}


