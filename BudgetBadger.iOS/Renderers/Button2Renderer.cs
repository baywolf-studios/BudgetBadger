using System;
using System.ComponentModel;
using BudgetBadger.Forms.UserControls;
using BudgetBadger.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Button2), typeof(Button2Renderer))]
namespace BudgetBadger.iOS.Renderers
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

            if (e?.OldElement != null)
            {
                this.Control.TouchCancel -= this.Control_Released;
                this.Control.TouchDragExit -= this.Control_Released;
            }

            if (e?.NewElement != null && e?.NewElement is Button2)
            {
                this.Control.TouchCancel += Control_Released;
                this.Control.TouchDragExit += Control_Released;
                _card = (Button2)e.NewElement;
                this.Elevate(_card.Elevation);
            }
        }

        void Control_Released(object sender, EventArgs e)
        {
            _card?.UpdateReleased();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e?.PropertyName == nameof(Button2.Elevation))
            {
                this.Elevate(_card.Elevation);
            }
        }
    }
}