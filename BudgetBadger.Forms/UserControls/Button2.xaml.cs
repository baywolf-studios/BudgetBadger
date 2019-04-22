using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Button2 : Button
    {
        double originalWidth = -1;
        Color backgroundColor;

        public static BindableProperty ElevationProperty = BindableProperty.Create(nameof(Elevation), typeof(float), typeof(Button2), 0.0f);
        public float Elevation
        {
            get
            {
                return (float)GetValue(ElevationProperty);
            }
            set
            {
                SetValue(ElevationProperty, value);
            }
        }

        public Button2()
        {
            InitializeComponent();

            backgroundColor = BackgroundColor;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Width) && Device.RuntimePlatform == Device.macOS && originalWidth < Width)
                {
                    WidthRequest = Width + Padding.HorizontalThickness;
                    originalWidth = WidthRequest;
                }
            };

            Pressed += (sender, e) =>
            {
                backgroundColor = BackgroundColor;

                if (backgroundColor != Color.Transparent)
                {
                    Elevation += 6;
                }

                if (Device.RuntimePlatform == Device.macOS || Device.RuntimePlatform == Device.iOS)
                {
                    if (backgroundColor == Color.Transparent)
                    {
                        BackgroundColor = (Color)Application.Current.Resources["SecondaryButtonActiveColor"];
                    }
                    else
                    {
                        BackgroundColor = Color.FromHsla(backgroundColor.Hue, backgroundColor.Saturation, 0.9 * backgroundColor.Luminosity, backgroundColor.A);
                    }
                }
            };
            Released += (sender, e) =>
            {
                if (Device.RuntimePlatform == Device.macOS || Device.RuntimePlatform == Device.iOS)
                {
                    BackgroundColor = backgroundColor;
                }

                if (backgroundColor != Color.Transparent)
                {
                    Elevation -= 6;
                }
            };
        }
    }
}
