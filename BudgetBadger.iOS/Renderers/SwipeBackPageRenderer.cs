using System;
using BudgetBadger.iOS.Renderers;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Page), typeof(SwipBackPageRenderer))]
namespace BudgetBadger.iOS.Renderers
{
    public class SwipBackPageRenderer : PageRenderer
    {
        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            ViewController.NavigationController.InteractivePopGestureRecognizer.Enabled = true;
            ViewController.NavigationController.InteractivePopGestureRecognizer.Delegate = new InteractivePopRecognizer(ViewController.NavigationController);
        }
    }

    public class InteractivePopRecognizer : UIGestureRecognizerDelegate
    {

        UINavigationController navigationController;

        public InteractivePopRecognizer(UINavigationController controller)
        {
            navigationController = controller;
        }

        public override bool ShouldBegin(UIGestureRecognizer recognizer)
        {
            return navigationController.ViewControllers.Length > 1;
        }

        public override bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }
    }
}

