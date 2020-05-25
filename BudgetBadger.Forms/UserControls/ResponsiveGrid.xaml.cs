using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ResponsiveGrid : Grid
    {
        public static BindableProperty GutterProperty =
            BindableProperty.Create(nameof(Gutter),
                typeof(double),
                typeof(ResponsiveGrid),
                defaultBindingMode: BindingMode.OneWayToSource);
        public double Gutter
        {
            get => (double)GetValue(GutterProperty);
            set => SetValue(GutterProperty, value);
        }

        public ResponsiveGrid()
        {
            InitializeComponent();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (Width > 0)
            {
                if (Width <= 768)
                {
                    Gutter = 16;
                }
                else if (Width <= 1280)
                {
                    Gutter = 24;
                }
                else if (Width <= 1768)
                {
                    Gutter = 32;
                }
                else if (Width <= 2160)
                {
                    Gutter = 40;
                }
                else
                {
                    Gutter = 48;
                }
            }
        }
    }
}
