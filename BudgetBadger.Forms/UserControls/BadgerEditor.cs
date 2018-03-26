using System;

using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public class BadgerEditor : Editor
    {
        public BadgerEditor()
        {
            this.TextChanged += OnTextChanged_Grow;
        }

        ~BadgerEditor()
        {
            this.TextChanged -= OnTextChanged_Grow;
        }

        void OnTextChanged_Grow(Object sender, TextChangedEventArgs e)
        {
            this.InvalidateMeasure();
        }
    }
}

