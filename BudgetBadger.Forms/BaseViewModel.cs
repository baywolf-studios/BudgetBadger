using System;
using Prism;
using Prism.Mvvm;

namespace BudgetBadger.Forms
{
    public class BaseViewModel : BindableBase, IActiveAware
    {
        private bool _isActive;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                if (value)
                    OnActivated();
                else
                    OnDeactivated();
            }
        }
        public event EventHandler IsActiveChanged;

        public virtual void OnActivated() { }
        public virtual void OnDeactivated() { }
    }
}
