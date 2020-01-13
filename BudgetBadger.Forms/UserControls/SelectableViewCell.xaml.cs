using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class SelectableViewCell : ViewCell
    {
        public static BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(nameof(SelectedBackgroundColor), typeof(Color), typeof(SelectableViewCell), Color.Accent);
        public Color SelectedBackgroundColor
        {
            get => (Color)GetValue(SelectedBackgroundColorProperty);
            set => SetValue(SelectedBackgroundColorProperty, value);
        }

        public SelectableViewCell()
        {
            InitializeComponent();
        }

        protected override void OnParentSet()
        {
            if (Parent is Xamarin.Forms.ListView xlistView)
            {
                if (xlistView.Parent is ListView2 listView)
                {
                    listView.ItemSelected += (sender, e) =>
                    {
                        if (BindingContext == e.SelectedItem)
                        {
                            View.BackgroundColor = SelectedBackgroundColor;
                        }
                        else
                        {
                            View.BackgroundColor = BackgroundColor;
                        }
                    };
                }
            }
        }
    }
}
