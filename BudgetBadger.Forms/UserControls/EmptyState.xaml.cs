using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class EmptyState : ContentView
    {
        public static readonly BindableProperty ShowHelperProperty = BindableProperty.Create(
            nameof(ShowHelper),
            typeof(bool),
            typeof(ContentView),
            false);

        public bool ShowHelper
        {
            get { return (bool)GetValue(ShowHelperProperty); }
            set { SetValue(ShowHelperProperty, value); }
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(ContentView),
            string.Empty);

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
            nameof(Subtitle),
            typeof(string),
            typeof(ContentView),
            string.Empty);

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        public static readonly BindableProperty IconProperty = BindableProperty.Create(
            nameof(Icon),
            typeof(string),
            typeof(ContentView),
            string.Empty);

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public EmptyState()
        {
            InitializeComponent();
        }
    }
}
