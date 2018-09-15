using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Envelopes;
using FFImageLoading.Svg.Forms;
using Prism.Ioc;
using Prism.Navigation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Views
{
    public partial class MainDesktopPage : MasterDetailPage
    {
        Color _backgroundColor
        {
            get => (Color)Application.Current.Resources["BackgroundColor"];
        }

        Color _secondaryTextColor
        {
            get => (Color)Application.Current.Resources["SecondaryTextColor"];
        }

        Color _primaryTextColor
        {
            get => (Color)Application.Current.Resources["PrimaryTextColor"];
        }

        string _replaceColorMap;

        void SetAllInactive()
        {
            EnvelopesFrame.BackgroundColor = _backgroundColor;
            AccountsFrame.BackgroundColor = _backgroundColor;
            PayeesFrame.BackgroundColor = _backgroundColor;
            ReportsFrame.BackgroundColor = _backgroundColor;
            SettingsFrame.BackgroundColor = _backgroundColor;

            EnvelopesIcon.ReplaceStringMap.Clear();
            EnvelopesIcon.ReplaceStringMap.Add(_replaceColorMap, _secondaryTextColor.GetHexString());
            EnvelopesIcon.ReloadImage();
            EnvelopesLabel.TextColor = _secondaryTextColor;

            AccountsIcon.ReplaceStringMap.Clear();
            AccountsIcon.ReplaceStringMap.Add(_replaceColorMap, _secondaryTextColor.GetHexString());
            AccountsIcon.ReloadImage();
            AccountsLabel.TextColor = _secondaryTextColor;

            PayeesIcon.ReplaceStringMap.Clear();
            PayeesIcon.ReplaceStringMap.Add(_replaceColorMap, _secondaryTextColor.GetHexString());
            PayeesIcon.ReloadImage();
            PayeesLabel.TextColor = _secondaryTextColor;

            ReportsIcon.ReplaceStringMap.Clear();
            ReportsIcon.ReplaceStringMap.Add(_replaceColorMap, _secondaryTextColor.GetHexString());
            ReportsIcon.ReloadImage();
            ReportsLabel.TextColor = _secondaryTextColor;

            SettingsIcon.ReplaceStringMap.Clear();
            SettingsIcon.ReplaceStringMap.Add(_replaceColorMap, _secondaryTextColor.GetHexString());
            SettingsIcon.ReloadImage();
            SettingsLabel.TextColor = _secondaryTextColor;
        }

        void Handle_Tapped(object sender, EventArgs e)
        {
            SetAllInactive();

            var frame = (ContentView)sender;
            frame.BackgroundColor = (Color)Application.Current.Resources["SelectedItemColor"];

            var stackLayout = (StackLayout)frame.Content;

            var currentIcon = (SvgCachedImage)stackLayout.Children.FirstOrDefault(c => c is SvgCachedImage);
            currentIcon.ReplaceStringMap.Clear();
            currentIcon.ReplaceStringMap.Add(_replaceColorMap, _primaryTextColor.GetHexString());
            currentIcon.ReloadImage();

            var currentLabel = (Label)stackLayout.Children.FirstOrDefault(c => c is Label);
            currentLabel.TextColor = _primaryTextColor;

        }

        public MainDesktopPage()
        {
            InitializeComponent();

            _replaceColorMap = EnvelopesIcon.ReplaceStringMap.FirstOrDefault().Key;
        }

    }
}
