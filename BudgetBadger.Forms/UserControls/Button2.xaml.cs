using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Button2 : Button
    {
        double originalWidth = -1;

        public static BindableProperty RestingBackgroundColorProperty = BindableProperty.Create(nameof(RestingBackgroundColor), typeof(Color), typeof(Button2), Color.Accent);
        public Color RestingBackgroundColor
        {
            get => (Color)GetValue(RestingBackgroundColorProperty);
            set => SetValue(RestingBackgroundColorProperty, value);
        }

        public static BindableProperty RestingBorderColorProperty = BindableProperty.Create(nameof(RestingBorderColor), typeof(Color), typeof(Button2), Color.Accent);
        public Color RestingBorderColor
        {
            get => (Color)GetValue(RestingBorderColorProperty);
            set => SetValue(RestingBorderColorProperty, value);
        }

        public static BindableProperty ActiveBackgroundColorProperty = BindableProperty.Create(nameof(ActiveBackgroundColor), typeof(Color), typeof(Button2), Color.Accent);
        public Color ActiveBackgroundColor
        {
            get => (Color)GetValue(ActiveBackgroundColorProperty);
            set => SetValue(ActiveBackgroundColorProperty, value);
        }

        public static BindableProperty ActiveBorderColorProperty = BindableProperty.Create(nameof(ActiveBorderColor), typeof(Color), typeof(Button2), Color.Accent);
        public Color ActiveBorderColor
        {
            get => (Color)GetValue(ActiveBorderColorProperty);
            set => SetValue(ActiveBorderColorProperty, value);
        }

        public Button2()
        {
            InitializeComponent();

            UpdateResting();

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Width) && Device.RuntimePlatform == Device.macOS && originalWidth < Width)
                {
                    WidthRequest = Width + Padding.HorizontalThickness;
                    originalWidth = WidthRequest;
                }

                if (e.PropertyName == nameof(RestingBackgroundColor)
                    || e.PropertyName == nameof(RestingBorderColor)
                    ||e.PropertyName == nameof(ActiveBackgroundColor)
                    || e.PropertyName == nameof(ActiveBorderColor)
                    || e.PropertyName == nameof(IsPressed))
                {
                    if (IsPressed)
                    {
                        UpdateActive();
                    }
                    else
                    {
                        UpdateResting();
                    }
                }
            };

            Pressed += (sender, e) =>
            {
                UpdateActive();
            };

            Released += (sender, e) =>
            {
                UpdateResting();
            };
        }

        void UpdateColors(Color backgroundColor, Color borderColor)
        {
            BackgroundColor = backgroundColor;
            BorderColor = borderColor;
        }

        public void UpdateResting()
        {
            UpdateColors(RestingBackgroundColor, RestingBorderColor);
        }

        public void UpdateActive()
        {
            UpdateColors(ActiveBackgroundColor, ActiveBorderColor);
        }
    }
}
