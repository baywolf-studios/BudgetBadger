using System;
using BudgetBadger.Forms.UserControls;
using BudgetBadger.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExpandingEditor), typeof(ExpandingEditorRenderer))]
namespace BudgetBadger.iOS.Renderers
{
    public class ExpandingEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.ScrollEnabled = false;
            }
        }
    }
}