using System;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public class ExpandingEditor : Editor
    {
        public ExpandingEditor()
        {
            TextChanged += (sender, e) =>
            {
                InvalidateMeasure();
            };
        }
    }
}
