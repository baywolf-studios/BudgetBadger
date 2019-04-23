using System;
using System.ComponentModel;
using Android.Animation;
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

        public static void Initialize()
        {
            // empty, but used for beating the linker
        }
    }
}
