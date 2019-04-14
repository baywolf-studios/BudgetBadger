using Syncfusion.SfDataGrid.XForms;
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

        public static BindableProperty SelectedCommandProperty = BindableProperty.Create(nameof(SelectedCommand), typeof(ICommand), typeof(ListView));
        public ICommand SelectedCommand
        {
            get => (ICommand)GetValue(SelectedCommandProperty);
            set => SetValue(SelectedCommandProperty, value);
        }

        public bool HasOtherTapGestureRecognizers { get; set; }

        public DataGrid ()
		{
			InitializeComponent ();
            //GridViewCreated += DataGrid_GridViewCreated;
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsBusy))
                {
                    ResetSwipeOffset();
                }
            };

            ScrollStateChanged += (sender, e) =>
            {
                ResetSwipeOffset();
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

                SelectionController.ClearSelection();
                ResetSwipeOffset();
            };
		}

        private void DataGrid_GridViewCreated(object sender, GridViewCreatedEventArgs e)
        {
            View.LiveDataUpdateMode = Syncfusion.Data.LiveDataUpdateMode.AllowDataShaping;
        }

        void UpdateFilter()
        {
            if (this != null && this.View != null && Filter != null)
            {
                this.View.Filter = Filter;
                this.View.RefreshFilter();
            }
        }
	}
}