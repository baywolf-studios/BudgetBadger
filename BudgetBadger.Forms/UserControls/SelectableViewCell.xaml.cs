using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class SelectableViewCell : ViewCell
    {
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
                            View.BackgroundColor = Color.AliceBlue;
                        }
                        else
                        {
                            View.BackgroundColor = Color.White;
                        }
                    };
                }
            }
        }
    }
}
