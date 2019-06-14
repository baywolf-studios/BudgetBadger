using Syncfusion.SfDataGrid.XForms;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BudgetBadger.Forms
{
    public class SfDataGridStyle : DataGridStyle
    {
        public override ImageSource GetHeaderSortIndicatorUp()
        {
            return null;
        }

        public override ImageSource GetHeaderSortIndicatorDown()
        {
            return null;
        }

        public override Color GetHeaderBackgroundColor()
        {
            return (Color)Application.Current.Resources["BackgroundColor"];
        }

        public override Color GetHeaderForegroundColor()
        {
            return (Color)Application.Current.Resources["SecondaryTextColor"];
        }

        public override Color GetCaptionSummaryRowBackgroundColor()
        {
            return (Color)Application.Current.Resources["BackgroundColor"];
        }

        public override Color GetCaptionSummaryRowForegroundColor()
        {
            return (Color)Application.Current.Resources["PrimaryTextColor"];
        }

        public override Color GetRecordBackgroundColor()
        {
            return (Color)Application.Current.Resources["BackgroundColor"];
        }

        public override Color GetRecordForegroundColor()
        {
            return (Color)Application.Current.Resources["PrimaryTextColor"];
        }

        public override Color GetSelectionForegroundColor()
        {
            return (Color)Application.Current.Resources["PrimaryTextColor"];
        }

        public override Color GetSelectionBackgroundColor()
        {
            return Color.Transparent;
        }

        public override Color GetBorderColor()
        {
            return (Color)Application.Current.Resources["DividerColor"];
        }

        public override GridLinesVisibility GetGridLinesVisibility()
        {
            return GridLinesVisibility.Both;
        }

        public override float GetBorderWidth()
        {
            return 1;
        }
    }
}
