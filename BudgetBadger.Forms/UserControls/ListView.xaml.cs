using Syncfusion.ListView.XForms;
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
	public partial class ListView : SfListView
	{
        public static BindableProperty FilterTextProperty = BindableProperty.Create(nameof(FilterText), typeof(string), typeof(ListView), propertyChanged: (bindable, oldVal, newVal) =>
        {
            ((ListView)bindable).UpdateFilter();
        });
        public string FilterText
        {
            get => (string)GetValue(FilterTextProperty);
            set => SetValue(FilterTextProperty, value);
        }

        public static BindableProperty FilterProperty = BindableProperty.Create(nameof(Filter), typeof(Predicate<object>), typeof(ListView));
        public Predicate<object> Filter
        {
            get => (Predicate<object>)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public ListView()
        {
            InitializeComponent();
        }

        private void UpdateFilter()
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                sfListView.DataSource.Filter = null;
            }
            else
            {
                sfListView.DataSource.Filter = Filter;
            }
            sfListView.DataSource.RefreshFilter();
        }
	}
}