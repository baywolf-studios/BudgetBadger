using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Style
{
    public class DynamicResourceProvider : INotifyPropertyChanged
    {
        public object this[string Key]
        {
            get
            {
                Application.Current.Resources.TryGetValue(Key, out object resource);

                if (resource is OnIdiom<double> onIdiom)
                {
                    return GetValue(onIdiom);
                }

                if (resource == null)
                {
                    //throw new Exception();
                }

                return resource;
            }
        }

        public static DynamicResourceProvider Instance { get; } = new DynamicResourceProvider();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Invalidate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        double GetValue(OnIdiom<double> onIdiom)
        {
            return onIdiom;
        }
    }
}
