using BudgetBadger.Forms.UserControls;
using BudgetBadger.UWP.Renderers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Card), typeof(CardRenderer))]
namespace BudgetBadger.UWP.Renderers
{
    public class CardRenderer :  ViewRenderer<Card, DropShadowPanel>
    {
        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        public CardRenderer()
        {
            AutoPackage = false;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            // We need an automation peer so we can interact with this in automated tests
            if (Control == null)
            {
                return new FrameworkElementAutomationPeer(this);
            }

            return new FrameworkElementAutomationPeer(Control);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Card> e)
        {
            if (e.NewElement != null)
            {
                if (Control == null)
                    SetNativeControl(new DropShadowPanel());

                PackChild();
                //UpdateBorder();
                UpdateDropShadow();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Card.ContentProperty.PropertyName)
            {
                PackChild();
            }
            else if (e.PropertyName == Card.BorderColorProperty.PropertyName || e.PropertyName == Card.HasShadowProperty.PropertyName)
            {
                //UpdateBorder();
            }
            else if (e.PropertyName == Card.ElevationProperty.PropertyName)
            {
                UpdateDropShadow();
            }
        }

        void PackChild()
        {
            if (Element.Content == null)
                return;

            IVisualElementRenderer renderer = Element.Content.GetOrCreateRenderer();
            Control.Content = renderer.ContainerElement;
        }

        void UpdateDropShadow()
        {
            Control.OffsetX = 0;
            Control.OffsetY = Element.Elevation < 10 ? Math.Floor(Element.Elevation / 2) + 1 : Element.Elevation - 4;
            Control.BlurRadius = Element.Elevation == 1 ? 3 : Element.Elevation * 2;
            Control.ShadowOpacity = (24 - Math.Round(Element.Elevation / 10)) / 100;
            Control.Color = Windows.UI.Colors.Black;
            Control.IsMasked = false;
        }
    }
}
