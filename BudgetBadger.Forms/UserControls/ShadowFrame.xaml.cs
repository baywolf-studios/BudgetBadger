using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ShadowFrame : Frame
    {
        public static BindableProperty ElevationProperty = BindableProperty.Create(nameof(Elevation), typeof(float), typeof(ShadowFrame), 4.0f);
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
                if (Device.RuntimePlatform == Device.iOS)
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
                if (Device.RuntimePlatform == Device.iOS)
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

        public ShadowFrame()
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
    }
}
