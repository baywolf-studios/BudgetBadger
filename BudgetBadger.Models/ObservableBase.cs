using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BudgetBadger.Models
{
    public class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.ReferenceEquals(property, value))
            {
                return false;
            }

            property = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}