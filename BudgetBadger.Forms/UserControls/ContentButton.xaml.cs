using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ContentButton : ContentView
    {
        public static BindableProperty RestingBackgroundColorProperty = BindableProperty.Create(nameof(RestingBackgroundColor), typeof(Color), typeof(ContentButton), Color.Accent);
        public Color RestingBackgroundColor
        {
            get => (Color)GetValue(RestingBackgroundColorProperty);
            set => SetValue(RestingBackgroundColorProperty, value);
        }

        public static BindableProperty HoverBackgroundColorProperty = BindableProperty.Create(nameof(HoverBackgroundColor), typeof(Color), typeof(ContentButton), Color.Accent);
        public Color HoverBackgroundColor
        {
            get => (Color)GetValue(HoverBackgroundColorProperty);
            set => SetValue(HoverBackgroundColorProperty, value);
        }

        public static BindableProperty ActiveBackgroundColorProperty = BindableProperty.Create(nameof(ActiveBackgroundColor), typeof(Color), typeof(ContentButton), Color.Accent);
        public Color ActiveBackgroundColor
        {
            get => (Color)GetValue(ActiveBackgroundColorProperty);
            set => SetValue(ActiveBackgroundColorProperty, value);
        }

        public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ContentButton));
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ContentButton));
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public static BindableProperty ForceActiveBackgroundProperty = BindableProperty.Create(nameof(ForceActiveBackground), typeof(bool), typeof(ContentButton), false);
        public bool ForceActiveBackground
        {
            get => (bool)GetValue(ForceActiveBackgroundProperty);
            set => SetValue(ForceActiveBackgroundProperty, value);
        }

        public event EventHandler<EventArgs> Tapped;

        public ContentButton()
        {
            InitializeComponent();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (sender, e) =>
            {
                UpdateActive();

                Tapped?.Invoke(this, new EventArgs());
                if (Command != null && Command.CanExecute(CommandParameter))
                {
                    Command.Execute(CommandParameter);
                }

                UpdateResting();
            };
            GestureRecognizers.Add(tapGestureRecognizer);

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ForceActiveBackground))
                {
                    if (!ForceActiveBackground)
                    {
                        UpdateResting();
                    }
                }
            };
        }

        public async void UpdateResting()
        {
            if (!ForceActiveBackground && BackgroundColor != RestingBackgroundColor)
            {
                uint animationLength = 150;
                var colorTask = this.ColorTo(ActiveBackgroundColor, RestingBackgroundColor, (Color obj2) => BackgroundColor = obj2, animationLength, Easing.CubicInOut);
                if (await Task.WhenAny(colorTask, Task.Delay((int)animationLength + 50)) != colorTask)
                {
                    ViewExtensions.CancelAnimations(this);
                    BackgroundColor = RestingBackgroundColor;
                }
            }
        }

        public void UpdateHover()
        {
            BackgroundColor = HoverBackgroundColor;
        }

        public void UpdateActive()
        {
            BackgroundColor = ActiveBackgroundColor;
        }
    }
}
