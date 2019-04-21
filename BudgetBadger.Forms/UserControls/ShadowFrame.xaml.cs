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

        public ShadowFrame()
        {
            InitializeComponent();
        }
    }
}
