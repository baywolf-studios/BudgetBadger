using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ViewCell : Xamarin.Forms.ViewCell
    {
        public static readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(ViewCell), default(Thickness),  propertyChanged: PaddingPropertyChanged);
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public static BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(SelectableViewCell), Color.Default);
        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public static readonly BindableProperty HeightRequestProperty = BindableProperty.Create(nameof(HeightRequest), typeof(double), typeof(ViewCell), default(double), propertyChanged: HeightPropertyChanged);
        public double HeightRequest
        {
            get { return (double)GetValue(HeightRequestProperty); }
            set { SetValue(HeightRequestProperty, value); }
        }

        public ViewCell()
        {
            InitializeComponent();

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(View))
                {
                    View.BackgroundColor = BackgroundColor;
                    View.HeightRequest = HeightRequest;

                    if (View is Layout layout)
                    {
                        layout.Padding = Padding;
                    }

                    if (Device.RuntimePlatform == Device.macOS)
                    {
                        View.Margin = new Thickness(-1);
                    }
                }
            };
        }

        static void PaddingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ViewCell viewCell && oldValue != newValue && newValue is Thickness thickness)
            {
                if (viewCell.View is Layout layout)
                {
                    layout.Padding = thickness;
                }
            }
        }

        static void HeightPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ViewCell viewCell && oldValue != newValue)
            {
                viewCell.Height = (double)newValue;
                if (viewCell.View != null)
                {
                    viewCell.View.HeightRequest = (double)newValue;
                }
            }
        }
    }
}
