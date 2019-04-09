﻿using System;
using AppKit;
using BudgetBadger.macOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(Button), typeof(CustomButtonRenderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class CustomButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                //Control.Bordered = false;
            }
        }
    }
}
