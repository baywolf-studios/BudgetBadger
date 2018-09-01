using Syncfusion.SfDataGrid.XForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.UserControls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DataGrid : SfDataGrid
    {
        public static BindableProperty FilterTextProperty = BindableProperty.Create(nameof(FilterText), typeof(string), typeof(DataGrid), propertyChanged: (bindable, oldVal, newVal) =>
        {
            ((DataGrid)bindable).UpdateFilter();
        });
        public string FilterText
        {
            get => (string)GetValue(FilterTextProperty);
            set => SetValue(FilterTextProperty, value);
        }

        public static BindableProperty FilterProperty = BindableProperty.Create(nameof(Filter), typeof(Predicate<object>), typeof(DataGrid));
        public Predicate<object> Filter
        {
            get => (Predicate<object>)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public DataGrid ()
		{
			InitializeComponent ();         
		}

        private void UpdateFilter()
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                sfGrid.View.Filter = null;
            }
            else
            {
                sfGrid.View.Filter = Filter;
            }
            sfGrid.View.RefreshFilter();
        }
	}
}