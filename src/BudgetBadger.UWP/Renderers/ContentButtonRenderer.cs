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

[assembly: ExportRenderer(typeof(ContentButton), typeof(ContentButtonRenderer))]
namespace BudgetBadger.UWP.Renderers
{
    public class ContentButtonRenderer : LayoutRenderer
    {
        private ContentButton _card;

        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
        {
            base.OnElementChanged(e);

            if (e?.NewElement != null && e?.NewElement is ContentButton)
            {
                this.AddHandler(PointerCanceledEvent, new PointerEventHandler(Control_Released), true);

                this.AddHandler(PointerEnteredEvent, new PointerEventHandler(Control_Hover), true);

                this.AddHandler(PointerExitedEvent, new PointerEventHandler(Control_Released), true);

                this.AddHandler(PointerPressedEvent, new PointerEventHandler(Control_Pressed), true);

                _card = (ContentButton)e.NewElement;
            }
        }

        void Control_Pressed(object sender, PointerRoutedEventArgs e)
        {
            _card?.UpdateActive();
        }

        void Control_Released(object sender, PointerRoutedEventArgs e)
        {
            _card?.UpdateResting();
        }

        void Control_Hover(object sender, PointerRoutedEventArgs e)
        {
            _card?.UpdateHover();
        }
    }
}
