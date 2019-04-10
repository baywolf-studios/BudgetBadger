using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Button2 : Button
    {
        double originalWidth = -1;

        public Button2()
        {
            InitializeComponent();

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Width) && Device.RuntimePlatform == Device.macOS && originalWidth < Width)
                {
                    WidthRequest = Width + Padding.HorizontalThickness;
                    originalWidth = WidthRequest;
                }
            };
        }
    }
}
