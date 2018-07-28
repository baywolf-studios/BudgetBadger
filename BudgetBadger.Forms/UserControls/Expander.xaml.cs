using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Expander : ContentView
    {
        uint _animationLength = 150;

        Color _idleColor
        {
            get => (Color)Application.Current.Resources["IdleColor"];
        }

        public static BindableProperty IsExpandedProperty =
            BindableProperty.Create(nameof(IsExpanded),
                                    typeof(bool),
                                    typeof(Expander),
                                    defaultValue: false,
                                    propertyChanged: (bindable, oldVal, newVal) =>
                                    {
                                        ((Expander)bindable).UpdateBodyVisibility((bool)newVal);
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
                return new Dictionary<string, string> { { "currentColor", _idleColor.GetHexString() } };
            }
        }

        public Expander()
        {
            InitializeComponent();
            IconControl.BindingContext = this;
        }

        async void UpdateBodyVisibility(bool isVisible)
        {
            if (isVisible)
            {
                // show the body
                BodyView.IsVisible = isVisible;
                await BodyView.FadeTo(1, _animationLength, Easing.CubicInOut);
            }
            else
            {
                // hide the body
                await BodyView.FadeTo(0, _animationLength, Easing.CubicInOut);
                BodyView.IsVisible = isVisible;

            }
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            IsExpanded = !IsExpanded;
        }
    }
}