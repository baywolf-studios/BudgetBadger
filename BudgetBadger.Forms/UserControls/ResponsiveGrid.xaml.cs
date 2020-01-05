using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ResponsiveGrid : Grid
    {
        public ResponsiveGrid()
        {
            InitializeComponent();
            SizeChanged += ResponsiveGrid_SizeChanged;
        }

        private void ResponsiveGrid_SizeChanged(object sender, EventArgs e)
        {
            if (Width > 0 && Device.RuntimePlatform != Device.macOS)
            {
                if (Width <= 304)
                {
                    //Padding = new Thickness(16,0,16,0);
                    ColumnSpacing = 16;
                }
                else if (Width <= 768)
                {
                    //Padding = new Thickness(24, 0, 24, 0);
                    ColumnSpacing = 24;
                }
                else if (Width <= 1280)
                {
                    //Padding = new Thickness(32, 0, 32, 0);
                    ColumnSpacing = 32;
                }
                else if (Width <= 1768)
                {
                    //Padding = new Thickness(40, 0, 40, 0);
                    ColumnSpacing = 40;
                }
                else
                {
                    //Padding = new Thickness(48, 0, 48, 0);
                    ColumnSpacing = 48;
                }
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            
        }
    }
}
