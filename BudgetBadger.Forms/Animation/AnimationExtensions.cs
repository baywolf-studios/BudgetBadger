using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Animation
{
    public static class AnimationExtensions
    {
        public static Task<bool> ColorTo(this VisualElement self, Color fromColor, Color toColor, Action<Color> callback, uint length = 250, Easing easing = null, string animationName = "ColorTo")
        {
            Color transform(double t) => Color.FromRgba(fromColor.R + t * (toColor.R - fromColor.R),
                                                        fromColor.G + t * (toColor.G - fromColor.G),
                                                        fromColor.B + t * (toColor.B - fromColor.B),
                                                        fromColor.A + t * (toColor.A - fromColor.A));
            easing = easing ?? Easing.Linear;

            var taskCompletionSource = new TaskCompletionSource<bool>();

            self.Animate(animationName, transform, callback, 16, length, easing, (v, c) => taskCompletionSource.SetResult(c));
            return taskCompletionSource.Task;
        }

        public static Task<bool> DoubleTo(this VisualElement self, double fromDouble, double toDouble, Action<double> callback, uint length = 250, Easing easing = null, string animationName = "DoubleTo")
        {
            double transform(double t) => fromDouble + t * (toDouble - fromDouble);

            easing = easing ?? Easing.Linear;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            self.Animate(animationName, transform, callback, 16, length, easing, (v, c) => taskCompletionSource.SetResult(c));
            return taskCompletionSource.Task;
        }
    }
}
