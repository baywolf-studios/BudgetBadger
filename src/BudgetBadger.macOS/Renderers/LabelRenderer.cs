using System;
using System.ComponentModel;
using BudgetBadger.macOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(Label), typeof(MacLabelRenderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class MacLabelRenderer : LabelRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Label.TextProperty.PropertyName)
            {
                Control.TextColor = Element.TextColor.ToNSColor();
            }

            base.OnElementPropertyChanged(sender, e);
        }
    }
}
