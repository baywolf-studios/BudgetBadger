using System;
using System.ComponentModel;
using BudgetBadger.Core.Localization;

namespace BudgetBadger.Forms.Style
{
    public class TranslationProvider : INotifyPropertyChanged
    {
        static readonly Lazy<IResourceContainer> _resourceContainer = new Lazy<IResourceContainer>(() => StaticResourceContainer.Current);

        public object this[string Text]
        {
            get
            {
                return _resourceContainer.Value.GetResourceString(Text);
            }
        }

        public static TranslationProvider Instance { get; } = new TranslationProvider();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Invalidate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
