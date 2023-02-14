using BudgetBadger.Core.Localization;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BudgetBadger.Forms
{
    public static class StaticResourceContainer
    {
        static Lazy<IResourceContainer> Implementation = new Lazy<IResourceContainer>(() => CreateResourceContainer(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        public static IResourceContainer Current
        {
            get
            {
                var ret = Implementation.Value;
                if (ret == null)
                {
                    throw new NotImplementedException("This functionality is not implemented yet.");
                }
                return ret;
            }
        }

        static IResourceContainer CreateResourceContainer()
        {
            return new ResourceContainer(DependencyService.Get<ILocalize>());
        }
    }
}
