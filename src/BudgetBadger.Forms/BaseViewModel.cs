using System;
using BudgetBadger.Core.Models;
using Prism;
using Prism.Mvvm;

namespace BudgetBadger.Forms
{
    public class BaseViewModel : ObservableBase, IActiveAware
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
