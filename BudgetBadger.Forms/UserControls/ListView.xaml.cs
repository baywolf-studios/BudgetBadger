using Syncfusion.ListView.XForms;
using Syncfusion.ListView.XForms.Control.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.UserControls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ListView : SfListView
	{
        double scrollY = 0;

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

        public static BindableProperty SelectedCommandProperty = BindableProperty.Create(nameof(SelectedCommand), typeof(ICommand), typeof(ListView));
        public ICommand SelectedCommand
        {
            get => (ICommand)GetValue(SelectedCommandProperty);
            set => SetValue(SelectedCommandProperty, value);
        }

        public bool HasOtherTapGestureRecognizers { get; set; }

        public ListView()
        {
            InitializeComponent();

            PropertyChanging += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsBusy))
                {
                    if (!IsBusy)
                    {
                        var scroller = this.GetScrollView();
                        scrollY = scroller.ScrollY;
                    }
                    else
                    {
                        ScrollTo(scrollY);
                    }
                }
            };

            SelectionChanging += (sender, e) => 
            {
                if (HasOtherTapGestureRecognizers && (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.macOS))
                {
                    e.Cancel = true;
                }
            };

            SelectionChanged += (sender, e) =>
            {
                if (SelectedCommand != null)
                {
                    SelectedCommand.Execute(e.AddedItems.FirstOrDefault());
                }
            };
        }

        void UpdateFilter()
        {
            if (this != null && this.DataSource != null && Filter != null)
            {
                this.DataSource.Filter = Filter;
                this.DataSource.RefreshFilter();
            } 
        }
	}
}