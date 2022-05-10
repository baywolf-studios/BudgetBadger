using System;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Style
{
    public static class Icons
    {
        public static string GetResource(string keyName)
        {
            var icon = string.Empty;
            if (Application.Current.Resources.TryGetValue(keyName, out object resource))
            {
                icon = (OnPlatform<string>)resource;
            }

            return icon;
        }

        public static string IconFontFamily
        {
            get => GetResource("IconFontFamily");
        }

        public static string Search
        {
            get => GetResource("SearchIcon");
        }

        public static string Close
        {
            get => GetResource("CloseIcon");
        }

        public static string ArrowBack
        {
            get => GetResource("ArrowBackIcon");
        }

        public static string ArrowForward
        {
            get => GetResource("ArrowForwardIcon");
        }

        public static string Envelope
        {
            get => GetResource("EnvelopeIcon");
        }

        public static string Account
        {
            get => GetResource("AccountIcon");
        }

        public static string Payee
        {
            get => GetResource("PayeeIcon");
        }

        public static string Report
        {
            get => GetResource("ReportIcon");
        }

        public static string Setting
        {
            get => GetResource("SettingIcon");
        }
    }
}
