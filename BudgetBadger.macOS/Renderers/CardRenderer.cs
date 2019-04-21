using System;
using System.ComponentModel;
using BudgetBadger.Forms.UserControls;
using BudgetBadger.macOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(Card), typeof(CardRenderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class CardRenderer : FrameRenderer
    {
        private Card _card;

        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            if (e?.NewElement != null && e?.NewElement is Card)
            {
                _card = (Card)e.NewElement;
                this.Elevate(_card.Elevation);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e?.PropertyName == nameof(Card.Elevation) || e?.PropertyName == nameof(Card.BackgroundColor))
            {
                this.Elevate(_card.Elevation);
            }
        }
    }
}
