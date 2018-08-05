using System;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
	public class SimpleDetailedDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SimpleTemplate { get; set; }
        public DataTemplate DetailedTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (Device.Idiom == TargetIdiom.Phone)
            {
                return SimpleTemplate;
            }

            return DetailedTemplate;
        }
    }
}
