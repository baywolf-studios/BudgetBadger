using System;
using AppKit;

namespace BudgetBadger.macOS.Renderers
{
    internal class BoolEventArgs : EventArgs
    {
        public BoolEventArgs(bool value)
        {
            Value = value;
        }
        public bool Value
        {
            get;
            private set;
        }
    }

    internal class FormsNSPopUpButton : NSPopUpButton
    {
        public EventHandler<BoolEventArgs> FocusChanged;

        public override bool ResignFirstResponder()
        {
            FocusChanged?.Invoke(this, new BoolEventArgs(false));
            return base.ResignFirstResponder();
        }
        public override bool BecomeFirstResponder()
        {
            FocusChanged?.Invoke(this, new BoolEventArgs(true));
            return base.BecomeFirstResponder();
        }
    }
}