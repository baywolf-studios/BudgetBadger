using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Syncfusion.SfPullToRefresh.XForms;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class PullToRefresh : ContentView
    {
        bool _automaticRefresh { get; set; }

        public static BindableProperty RefreshCommandProperty = BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(PullToRefresh), defaultBindingMode: BindingMode.TwoWay);
        public ICommand RefreshCommand
        {
            get => (ICommand)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }

        public static BindableProperty IsBusyProperty = BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(PullToRefresh));
        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public View PullableContent
        {
            get => sfPull.PullableContent;
            set => sfPull.PullableContent = value;
        }

        public PullToRefresh()
        {
            InitializeComponent();

            sfPull.IsRefreshing = false;
            activityIndicator.IsVisible = false;
            sfPull.IsVisible = true;

            sfPull.Refreshing += PullToRefresh_Refreshing;
            PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == nameof(IsBusy))
                {
                    if (!IsBusy)
                    {
                        sfPull.IsRefreshing = false;
                        activityIndicator.IsVisible = false;
                        sfPull.IsVisible = true;
                    }
                    else if (!sfPull.IsRefreshing)
                    {
                        sfPull.IsVisible = false;
                        activityIndicator.IsVisible = true;
                    }
                }
            };
        }

        void PullToRefresh_Refreshing(object sender, EventArgs e)
        {
            if (RefreshCommand != null && RefreshCommand.CanExecute(null))
            {
                _automaticRefresh = true;
                RefreshCommand.Execute(null);
                _automaticRefresh = false;
            }
        }
    }
}
