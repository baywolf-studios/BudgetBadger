using System;
using System.Collections.Generic;
using System.Windows.Input;
using Syncfusion.SfPullToRefresh.XForms;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class PullToRefresh : SfPullToRefresh
    {
        public static BindableProperty RefreshCommandProperty = BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(PullToRefresh), defaultBindingMode: BindingMode.TwoWay);
        public ICommand RefreshCommand
        {
            get => (ICommand)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }

        public static BindableProperty IsBusyProperty = BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(PullToRefresh), propertyChanged: (bindable, oldVal, newVal) =>
        {
            if ((bool)oldVal != (bool)newVal && (bool)newVal != ((PullToRefresh)bindable).IsRefreshing)
            {
                if ((bool)newVal == true)
                {
                    ((PullToRefresh)bindable).StartRefreshing();
                }
                else
                {
                    ((PullToRefresh)bindable).EndRefreshing();
                }
            }
        });
        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public PullToRefresh()
        {
            InitializeComponent();

            Refreshing += PullToRefresh_Refreshing;
        }

        void PullToRefresh_Refreshing(object sender, EventArgs e)
        {
            if (RefreshCommand != null)
            {
                RefreshCommand.Execute(null);
            }
        }

    }
}
