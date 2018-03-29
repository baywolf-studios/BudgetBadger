using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Animations
{
    public static class FontSizeExtensions
    {
        public static Task<bool> FontSizeTo(this VisualElement self, double fromFontSize, double toFontSize, Action<double> callback, uint length = 250, Easing easing = null)
        {
            Func<double, double> transform = (t) => fromFontSize + t * (toFontSize - fromFontSize);

            return FontAnimation(self, "FontSizeTo", transform, callback, length, easing);
        }

        public static void CancelAnimation(this VisualElement self)
        {
            self.AbortAnimation("FontSizeTo");
        }

        static Task<bool> FontAnimation(VisualElement element, string name, Func<double, double> transform, Action<double> callback, uint length, Easing easing)
        {
            easing = easing ?? Easing.Linear;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            element.Animate<double>(name, transform, callback, 16, length, easing, (v, c) => taskCompletionSource.SetResult(c));
            return taskCompletionSource.Task;
        }
    }
}
