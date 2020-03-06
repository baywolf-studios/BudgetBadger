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
            get => pullToRefresh.Content;
            set => pullToRefresh.Content = value;
        }

        public PullToRefresh()
        {
            InitializeComponent();

            //pullToRefresh.IsRefreshing = false;
            //activityIndicator.IsVisible = false;
            //pullToRefresh.IsVisible = true;

            //pullToRefresh.Refreshing += PullToRefresh_Refreshing;
            //PropertyChanged += (sender, e) => 
            //{
            //    if (e.PropertyName == nameof(IsBusy))
            //    {
            //        if (!IsBusy)
            //        {
            //            pullToRefresh.IsRefreshing = false;
            //            activityIndicator.IsVisible = false;
            //            pullToRefresh.IsVisible = true;
            //        }
            //        else if (!pullToRefresh.IsRefreshing)
            //        {
            //            pullToRefresh.IsVisible = false;
            //            activityIndicator.IsVisible = true;
            //        }
            //    }
            //};
        }

        void PullToRefresh_Refreshing(object sender, EventArgs e)
        {
            if (RefreshCommand != null && RefreshCommand.CanExecute(null))
            {
                RefreshCommand.Execute(null);
            }
        }
    }
}
