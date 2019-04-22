using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Button2 : Button
    {
        double originalWidth = -1;
        Color backgroundColor;
        Color pressedBackgroundColor;
        float elevation;
        float pressedElevation;

        public static BindableProperty IsRaisedProperty = BindableProperty.Create(nameof(IsRaised), typeof(bool), typeof(Button2), false);
        public bool IsRaised
        {
            get
            {
                return (bool)GetValue(IsRaisedProperty);
            }
            set
            {
                SetValue(IsRaisedProperty, value);
            }
        }

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
            pressedBackgroundColor = Color.FromHsla(backgroundColor.Hue, backgroundColor.Saturation, 0.9 * backgroundColor.Luminosity, backgroundColor.A);
            pressedElevation = Elevation;

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
                if (BackgroundColor != pressedBackgroundColor)
                {
                    backgroundColor = BackgroundColor;

                    if (IsRaised)
                    {
                        pressedBackgroundColor = Color.FromHsla(backgroundColor.Hue, backgroundColor.Saturation, 0.9 * backgroundColor.Luminosity, backgroundColor.A);

                    }
                    else
                    {
                        pressedBackgroundColor = (Color)Application.Current.Resources["SecondaryButtonActiveColor"];
                    }

                    BackgroundColor = pressedBackgroundColor;
                }

                if (IsRaised && Elevation != pressedElevation)
                {
                    elevation = Elevation;
                    pressedElevation = Elevation + 6;
                    Elevation = pressedElevation;
                }
            };

            Released += (sender, e) =>
            {
                UpdateReleased();
            };
        }

        public void UpdateReleased()
        {
            BackgroundColor = backgroundColor;
            Elevation = elevation;
        }
    }
}
