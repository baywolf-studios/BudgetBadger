using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Accordion : Grid
    {
        uint _animationLength = 150;

        Color _idleColor
        {
            get => (Color)Application.Current.Resources["IdleColor"];
        }

        public static BindableProperty IsExpandedProperty =
            BindableProperty.Create(nameof(IsExpanded),
                                    typeof(bool),
                                    typeof(Accordion),
                                    defaultValue: false,
                                    propertyChanged: (bindable, oldVal, newVal) =>
                                    {
                                        ((Accordion)bindable).UpdateBodyVisibility((bool)newVal);
                                    });

        public bool IsExpanded
        {
            get
            {
                return (bool)GetValue(IsExpandedProperty);
            }
            set
            {
                SetValue(IsExpandedProperty, value);
            }
        }

        public View HeaderContent
        {
            get => HeaderView.Content;
            set => HeaderView.Content = value;
        }

        public View BodyContent
        {
            get => BodyView.Content;
            set => BodyView.Content = value;
        }

        public Dictionary<string, string> ReplaceColor
        {
            get
            {
                return new Dictionary<string, string> { { "#ffffff", _idleColor.GetHexString() } };
            }
        }

        public Accordion()
        {
            InitializeComponent();
            IconControl.BindingContext = this;

            BodyView.IsVisible = false;
        }

        async void UpdateBodyVisibility(bool isVisible)
        {
            if (isVisible)
            {
                // show the body
                BodyView.IsVisible = isVisible;
                var fadeTask = BodyView.FadeTo(1, _animationLength, Easing.CubicInOut);
                var rotateTask = IconControl.RotateTo(90, _animationLength, Easing.CubicInOut);
                var combinedTask = Task.WhenAll(fadeTask, rotateTask);

                if (await Task.WhenAny(combinedTask, Task.Delay((int)_animationLength + 50)) != combinedTask)
                {
                    ViewExtensions.CancelAnimations(BodyView);
                    ViewExtensions.CancelAnimations(IconControl);
                    BodyView.Opacity = 1;
                    IconControl.Rotation = 90;
                }
            }
            else
            {
                // hide the body
                var fadeTask = BodyView.FadeTo(0, _animationLength, Easing.CubicInOut);
                var rotateTask = IconControl.RotateTo(00, _animationLength, Easing.CubicInOut);
                var combinedTask = Task.WhenAll(fadeTask, rotateTask);

                if (await Task.WhenAny(combinedTask, Task.Delay((int)_animationLength + 50)) != combinedTask)
                {
                    ViewExtensions.CancelAnimations(BodyView);
                    ViewExtensions.CancelAnimations(IconControl);
                    BodyView.Opacity = 0;
                    IconControl.Rotation = 0;
                }
                BodyView.IsVisible = isVisible;

            }
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            IsExpanded = !IsExpanded;
        }
    }
}