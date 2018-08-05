using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class EnvelopeViewCell : ViewCell
    {
        public static readonly BindableProperty ParentContextProperty =
            BindableProperty.Create("ParentContext", typeof(object), typeof(EnvelopeViewCell), null);

        public object ParentContext
        {
            get { return GetValue(ParentContextProperty); }
            set { SetValue(ParentContextProperty, value); }
        }

        public EnvelopeViewCell()
        {
            InitializeComponent();
        }
    }
}
