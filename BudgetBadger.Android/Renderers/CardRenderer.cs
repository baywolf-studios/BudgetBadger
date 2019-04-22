using System;
using System.ComponentModel;
using Android.Content;
using Android.Support.V4.View;
using BudgetBadger.Droid.Renderers;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Card), typeof(CardRenderer))]
namespace BudgetBadger.Droid.Renderers
{
    public class CardRenderer : Xamarin.Forms.Platform.Android.AppCompat.FrameRenderer
    {
        public CardRenderer(Context context) : base(context)
        {
        }

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
                UpdateElevation();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e?.PropertyName == nameof(Card.Elevation))
            {
                UpdateElevation();
            }
        }

        private void UpdateElevation()
        {
            StateListAnimator = null;

            // set the elevation manually
            ViewCompat.SetElevation(this, _card.Elevation);
            ViewCompat.SetElevation(Control, _card.Elevation);
        }
    }
}
