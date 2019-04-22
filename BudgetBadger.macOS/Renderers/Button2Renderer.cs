using System;
using System.ComponentModel;
using AppKit;
using BudgetBadger.Forms.UserControls;
using BudgetBadger.macOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(Button2), typeof(Button2Renderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class Button2Renderer : ButtonRenderer
    {
        private Button2 _card;

        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (e?.NewElement != null && e?.NewElement is Button2)
            {
                _card = (Button2)e.NewElement;
                UpdateBorder();
                this.Elevate(_card.Elevation);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e?.PropertyName == nameof(Button2.BorderWidth) || e?.PropertyName == nameof(Button2.BorderColor))
            {
                UpdateBorder();
            }

            if (e?.PropertyName == nameof(Button2.Elevation))
            {
                this.Elevate(_card.Elevation);
            }
        }

        void UpdateBorder()
        {
            if (Control != null)
            {
                Control.Bordered = false;
            }
        }
    }
}
