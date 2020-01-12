using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ViewCell : Xamarin.Forms.ViewCell
    {
        public static BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(SelectableViewCell), Color.Default);
        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public ViewCell()
        {
            InitializeComponent();

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(View))
                {
                    View.BackgroundColor = BackgroundColor;
                    if (Device.RuntimePlatform == Device.macOS)
                    {
                        View.Margin = new Thickness(-1);
                    }
                }
            };
        }
    }
}
