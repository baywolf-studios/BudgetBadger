using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Card : Frame
    {
        public static BindableProperty ElevationProperty = BindableProperty.Create(nameof(Elevation), typeof(float), typeof(Card), 4.0f);
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

        Frame iosInnerFrame = new Frame { Padding = 0, Margin = 0, HasShadow = false, IsClippedToBounds = true };
        public View Body
        {
            get
            {
                if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.macOS)
                {
                    return iosInnerFrame.Content;
                }
                else
                {
                    return Content;
                }
            }
            set
            {
                if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.macOS)
                {
                    iosInnerFrame.Content = value;
                    Content = iosInnerFrame;
                }
                else
                {
                    Content = value;
                }
            }
        }

        public Card()
        {
            InitializeComponent();

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(CornerRadius))
                {
                    iosInnerFrame.CornerRadius = CornerRadius;
                }
            };
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(HasShadow))
            {
                return;
            }

            base.OnPropertyChanged(propertyName);
        }
    }
}
