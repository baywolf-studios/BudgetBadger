using BudgetBadger.Forms.UserControls;
using BudgetBadger.UWP.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Button2), typeof(Button2Renderer))]
namespace BudgetBadger.UWP.Renderers
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
                this.Control.AddHandler(PointerReleasedEvent, new PointerEventHandler(Control_Released), true);
                this.Control.AddHandler(PointerCanceledEvent, new PointerEventHandler(Control_Released), true);
                this.Control.AddHandler(PointerReleasedEvent, new PointerEventHandler(Control_Released), true);
               
                _card = (Button2)e.NewElement;
            }
        }

        void Control_Released(object sender, PointerRoutedEventArgs e)
        {
            _card?.UpdateReleased();
        }
    }
}
