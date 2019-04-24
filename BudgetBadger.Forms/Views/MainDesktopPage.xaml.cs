using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Envelopes;
using BudgetBadger.Forms.UserControls;
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

        void SetAllInactive(Guid excluded)
        {
            if (excluded != EnvelopesFrame.Id)
            {
                EnvelopesFrame.BackgroundColor = EnvelopesFrame.RestingBackgroundColor;
                EnvelopesIcon.ReplaceStringMap.Clear();
                EnvelopesIcon.ReplaceStringMap.Add(_replaceColorMap, _secondaryTextColor.GetHexString());
                EnvelopesIcon.ReloadImage();
                EnvelopesLabel.TextColor = _secondaryTextColor;
            }

            if (excluded != AccountsFrame.Id)
            {
                AccountsFrame.BackgroundColor = AccountsFrame.RestingBackgroundColor;
                AccountsIcon.ReplaceStringMap.Clear();
                AccountsIcon.ReplaceStringMap.Add(_replaceColorMap, _secondaryTextColor.GetHexString());
                AccountsIcon.ReloadImage();
                AccountsLabel.TextColor = _secondaryTextColor;
            }

            if (excluded != PayeesFrame.Id)
            {
                PayeesFrame.BackgroundColor = PayeesFrame.RestingBackgroundColor;
                PayeesIcon.ReplaceStringMap.Clear();
                PayeesIcon.ReplaceStringMap.Add(_replaceColorMap, _secondaryTextColor.GetHexString());
                PayeesIcon.ReloadImage();
                PayeesLabel.TextColor = _secondaryTextColor;
            }

            if (excluded != ReportsFrame.Id)
            {
                ReportsFrame.BackgroundColor = ReportsFrame.RestingBackgroundColor;
                ReportsIcon.ReplaceStringMap.Clear();
                ReportsIcon.ReplaceStringMap.Add(_replaceColorMap, _secondaryTextColor.GetHexString());
                ReportsIcon.ReloadImage();
                ReportsLabel.TextColor = _secondaryTextColor;
            }

            if (excluded != SettingsFrame.Id)
            {
                SettingsFrame.BackgroundColor = SettingsFrame.RestingBackgroundColor;
                SettingsIcon.ReplaceStringMap.Clear();
                SettingsIcon.ReplaceStringMap.Add(_replaceColorMap, _secondaryTextColor.GetHexString());
                SettingsIcon.ReloadImage();
                SettingsLabel.TextColor = _secondaryTextColor;
            }
        }

        void Handle_Tapped(object sender, EventArgs e)
        {
            var frame = (ContentButton)sender;

            SetAllInactive(frame.Id);

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
