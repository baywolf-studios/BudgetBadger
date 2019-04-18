using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class HelperControl : ContentView
    {
        public static readonly BindableProperty ShowHelperProperty = BindableProperty.Create(
            "ShowHelper",
            typeof(bool),
            typeof(ContentView),
            false);

        public bool ShowHelper
        {
            get { return (bool)GetValue(ShowHelperProperty); }
            set { SetValue(ShowHelperProperty, value); }
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            "Title",
            typeof(string),
            typeof(ContentView),
            string.Empty);

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
            "Subtitle",
            typeof(string),
            typeof(ContentView),
            string.Empty);

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        public static readonly BindableProperty ImagePathProperty = BindableProperty.Create(
            "ImagePath",
            typeof(string),
            typeof(ContentView),
            string.Empty);

        public string ImagePath
        {
            get { return (string)GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }

        public HelperControl()
        {
            InitializeComponent();
        }
    }
}
