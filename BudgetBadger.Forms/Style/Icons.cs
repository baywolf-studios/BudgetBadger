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

        public static string Cancel
        {
            get => GetResource("CancelIcon");
        }
    }
}
