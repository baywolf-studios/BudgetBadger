using System;
using System.ComponentModel;
using Android.Content;
using Android.Support.V4.View;
using BudgetBadger.Droid.Renderers;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Button2), typeof(Button2Renderer))]
namespace BudgetBadger.Droid.Renderers
{
    public class Button2Renderer : Xamarin.Forms.Platform.Android.AppCompat.ButtonRenderer
    {
        public Button2Renderer(Context context) : base(context)
        {
        }

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
            // set the elevation manually
            ViewCompat.SetElevation(this, _card.Elevation);
            ViewCompat.SetElevation(Control, _card.Elevation);
        }
    }
}
