using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using BudgetBadger.Models;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ListView2 : StackLayout
    {
        public static readonly BindableProperty HeaderProperty = BindableProperty.Create(nameof(Header), typeof(object), typeof(ListView), null, propertyChanged: UpdateHeader);
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly BindableProperty HeaderTemplateProperty = BindableProperty.Create(nameof(HeaderTemplate), typeof(DataTemplate), typeof(ListView), null, propertyChanged: UpdateHeader);
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public static BindableProperty IsHeaderStickyProperty = BindableProperty.Create(nameof(IsHeaderSticky), typeof(bool), typeof(ListView2), propertyChanged: UpdateHeader);
        public bool IsHeaderSticky
        {
            get => (bool)GetValue(IsHeaderStickyProperty);
            set => SetValue(IsHeaderStickyProperty, value);
        }

        public static readonly BindableProperty FooterProperty = BindableProperty.Create(nameof(Footer), typeof(object), typeof(ListView), null, propertyChanged: UpdateFooter);
        public object Footer
        {
            get { return GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly BindableProperty FooterTemplateProperty = BindableProperty.Create(nameof(FooterTemplate), typeof(DataTemplate), typeof(ListView), null, propertyChanged: UpdateFooter);
        public DataTemplate FooterTemplate
        {
            get { return (DataTemplate)GetValue(FooterTemplateProperty); }
            set { SetValue(FooterTemplateProperty, value); }
        }

        public static BindableProperty IsFooterStickyProperty = BindableProperty.Create(nameof(IsFooterSticky), typeof(bool), typeof(ListView2), propertyChanged: UpdateFooter);
        public bool IsFooterSticky
        {
            get => (bool)GetValue(IsFooterStickyProperty);
            set => SetValue(IsFooterStickyProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(ListView), null, BindingMode.OneWayToSource);
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly BindableProperty SelectedCommandProperty = BindableProperty.Create(nameof(SelectedCommand), typeof(ICommand), typeof(ListView), null);
        public ICommand SelectedCommand
        {
            get { return (ICommand)GetValue(SelectedCommandProperty); }
            set { SetValue(SelectedCommandProperty, value); }
        }

        public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(nameof(SelectionMode), typeof(ListViewSelectionMode), typeof(ListView), ListViewSelectionMode.Single);
        public ListViewSelectionMode SelectionMode
        {
            get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        public static readonly BindableProperty HasUnevenRowsProperty = BindableProperty.Create(nameof(HasUnevenRows), typeof(bool), typeof(ListView), false);
        public bool HasUnevenRows
        {
            get { return (bool)GetValue(HasUnevenRowsProperty); }
            set { SetValue(HasUnevenRowsProperty, value); }
        }

        public static readonly BindableProperty RowHeightProperty = BindableProperty.Create(nameof(RowHeight), typeof(int), typeof(ListView), -1);
        public int RowHeight
        {
            get { return (int)GetValue(RowHeightProperty); }
            set { SetValue(RowHeightProperty, value); }
        }

        public static readonly BindableProperty SeparatorVisibilityProperty = BindableProperty.Create(nameof(SeparatorVisibility), typeof(SeparatorVisibility), typeof(ListView), SeparatorVisibility.Default);
        public SeparatorVisibility SeparatorVisibility
        {
            get { return (SeparatorVisibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        public static readonly BindableProperty SeparatorColorProperty = BindableProperty.Create(nameof(SeparatorColor), typeof(Color), typeof(ListView), Color.Default);
        public Color SeparatorColor
        {
            get { return (Color)GetValue(SeparatorColorProperty); }
            set { SetValue(SeparatorColorProperty, value); }
        }

        public static readonly BindableProperty HorizontalScrollBarVisibilityProperty = BindableProperty.Create(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ListView), ScrollBarVisibility.Default);
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        public static readonly BindableProperty VerticalScrollBarVisibilityProperty = BindableProperty.Create(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ListView), ScrollBarVisibility.Default);
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        public static readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IEnumerable), typeof(ListView2), null, propertyChanged: OnItemsChanged);
        public IEnumerable Items
        {
            get => (IEnumerable)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ListView2), null, propertyChanged: OnItemTemplateChanged);
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static BindableProperty SearchFilterProperty = BindableProperty.Create(nameof(SearchFilter), typeof(Predicate<object>), typeof(ListView2), propertyChanged: UpdateItems);
        public Predicate<object> SearchFilter
        {
            get => (Predicate<object>)GetValue(SearchFilterProperty);
            set => SetValue(SearchFilterProperty, value);
        }

        public static BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(ListView2), propertyChanged: UpdateItems);
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static BindableProperty IsGroupedProperty = BindableProperty.Create(nameof(IsGrouped), typeof(bool), typeof(ListView2), propertyChanged: UpdateGrouped);
        public bool IsGrouped
        {
            get => (bool)GetValue(IsGroupedProperty);
            set => SetValue(IsGroupedProperty, value);
        }

        public static BindableProperty GroupPropertyDescriptionProperty = BindableProperty.Create(nameof(GroupPropertyDescription), typeof(string), typeof(ListView2));
        public string GroupPropertyDescription
        {
            get => (string)GetValue(GroupPropertyDescriptionProperty);
            set => SetValue(GroupPropertyDescriptionProperty, value);
        }

        public static readonly BindableProperty GroupHeaderTemplateProperty = BindableProperty.Create(nameof(GroupHeaderTemplate), typeof(DataTemplate), typeof(ListView2), null, propertyChanged: OnGroupHeaderTemplateChanged);
        public DataTemplate GroupHeaderTemplate
        {
            get { return (DataTemplate)GetValue(GroupHeaderTemplateProperty); }
            set { SetValue(GroupHeaderTemplateProperty, value); }
        }

        public static BindableProperty IsSortedProperty = BindableProperty.Create(nameof(IsSorted), typeof(bool), typeof(ListView2), propertyChanged: UpdateItems);
        public bool IsSorted
        {
            get => (bool)GetValue(IsSortedProperty);
            set => SetValue(IsSortedProperty, value);
        }

        public static BindableProperty SortComparerProperty = BindableProperty.Create(nameof(SortComparer), typeof(IComparer<Object>), typeof(ListView2));
        public IComparer<Object> SortComparer
        {
            get => (IComparer<Object>)GetValue(SortComparerProperty);
            set => SetValue(SortComparerProperty, value);
        }

        private double previousY;
        private double minY;
        private double maxY;
        private bool atStart = true;
        private bool atEnd;
        private bool scrollingToStart;
        private DateTime lastScrollChange = DateTime.Now;
        private DateTime lastSearchBarChange = DateTime.Now;

        public ListView2()
        {
            InitializeComponent();
            SearchBar.BindingContext = this;
            InternalListView.BindingContext = this;
            InternalListView.ItemsSource = new ObservableList<object>();
            InternalListView.ItemSelected += InternalListView_ItemSelected;
            InternalListView.Scrolled += InternalListView_Scrolled;
        }

        private void InternalListView_Scrolled(object sender, ScrolledEventArgs e)
        {
            //set the maxY and minY to determine the start and end
            if (e.ScrollY > maxY)
            {
                maxY = e.ScrollY;
            }
            if (e.ScrollY < minY)
            {
                minY = e.ScrollY;
            }

            //check if it's close enough to the end
            if (CloseTo(e.ScrollY, maxY))
            {
                atEnd = true;
            }
            else
            {
                atEnd = false;
            }

            //check if it's close enough to the beginning
            if (CloseTo(minY, e.ScrollY))
            {
                atStart = true;
            }
            else
            {
                atStart = false;
            }

            // determine if we're scrolling to the top
            if (previousY > e.ScrollY)
            {
                scrollingToStart = true;
            }
            else
            {
                scrollingToStart = false;
            }
            previousY = e.ScrollY;

            var newSearchBarIsVisible = !atEnd && (atStart || scrollingToStart);
            var shouldChange = (SearchBar.IsVisible != newSearchBarIsVisible && lastSearchBarChange < DateTime.Now);

            if (shouldChange)
            {
                SearchBar.IsVisible = newSearchBarIsVisible;
                lastSearchBarChange = DateTime.Now.AddMilliseconds(250);
            }
        }

        private bool CloseTo(double current, double maximum)
        {
            return Math.Abs((Math.Abs(current) - Math.Abs(maximum)) / Math.Abs(maximum)) < 0.15;
        }

        private void InternalListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (sender is ListView2)
            {
                if (SelectedCommand != null && SelectedCommand.CanExecute(e.SelectedItem))
                {
                    SelectedCommand.Execute(e.SelectedItem);
                }
            }
        }

        private static void UpdateHeader(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                if (listView.IsHeaderSticky)
                {
                    listView.InternalListView.Header = null;
                    listView.InternalListView.HeaderTemplate = null;

                    if (listView.HeaderTemplate != null)
                    {
                        var headerView = (View)listView.HeaderTemplate.CreateContent();
                        headerView.BindingContext = listView.Header;
                        listView.stickyHeader.Content = headerView;
                    }
                    else
                    {
                        listView.stickyHeader.Content = (View)listView.Header;
                    }

                    listView.stickyHeader.IsVisible = true;
                }
                else
                {
                    listView.stickyHeader.IsVisible = false;
                    listView.InternalListView.Header = listView.Header;
                    listView.InternalListView.HeaderTemplate = listView.HeaderTemplate;
                }
            }
        }

        private static void UpdateFooter(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                if (listView.IsFooterSticky || Device.RuntimePlatform == Device.macOS)
                {
                    listView.InternalListView.Footer = null;
                    listView.InternalListView.FooterTemplate = null;

                    if (listView.FooterTemplate != null)
                    {
                        var footerView = (View)listView.FooterTemplate.CreateContent();
                        footerView.BindingContext = listView.Footer;
                        listView.stickyFooter.Content = footerView;
                    }
                    else
                    {
                        listView.stickyFooter.Content = (View)listView.Footer;
                    }

                    listView.stickyFooter.IsVisible = true;
                }
                else
                {
                    listView.stickyFooter.IsVisible = false;
                    listView.InternalListView.Footer = listView.Footer;
                    listView.InternalListView.FooterTemplate = listView.FooterTemplate;
                }
            }
        }

        private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.InternalListView.ItemTemplate = (DataTemplate)newValue;
            }
        }

        private static void OnGroupHeaderTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.InternalListView.GroupHeaderTemplate = (DataTemplate)newValue;
            }
        }

        private static void OnItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                if (newValue is INotifyCollectionChanged newCollection)
                {
                    newCollection.CollectionChanged += (sender, e) =>
                    {
                        listView.UpdateItems();
                    };
                }

                listView.UpdateItems();
            }
        }

        private static void UpdateGrouped(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                if ((bool)newValue)
                {
                    listView.InternalListView.ItemsSource = new ObservableList<ObservableGrouping<object, object>>();
                    listView.InternalListView.IsGroupingEnabled = listView.IsGrouped;
                    listView.InternalListView.GroupDisplayBinding = new Binding("Key");
                }
                else
                {
                    listView.InternalListView.ItemsSource = new ObservableList<object>();
                    listView.InternalListView.IsGroupingEnabled = listView.IsGrouped;
                    listView.InternalListView.GroupDisplayBinding = null;
                }
                
                listView.UpdateItems();
            }
        }

        private static void UpdateItems(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.UpdateItems();
            }
        }

        private void UpdateItems()
        {
            if (Items == null)
            {
                if (IsGrouped)
                {
                    ((ObservableList<ObservableGrouping<object, object>>)InternalListView.ItemsSource).RemoveAll();
                }
                else
                {
                    ((ObservableList<object>)InternalListView.ItemsSource).RemoveAll();
                }
            }
            else
            {
                var filteredItems = new List<object>();

                // filtering
                if (SearchFilter != null)
                {
                    foreach (var item in Items)
                    {
                        if (SearchFilter.Invoke(item))
                        {
                            filteredItems.Add(item);
                        }
                    }
                }
                else
                {
                    var itemSource = Items?.Cast<object>().ToList();
                    filteredItems.AddRange(itemSource);
                }

                //grouping
                if (IsGrouped)
                {
                    var groupedItems = filteredItems.GroupBy(g => GetPropertyValue(g, GroupPropertyDescription));

                    var sourceGroupedItems = groupedItems.ToDictionary(a => a.Key, a2 => new ObservableGrouping<object, object>(a2.Key, a2));
                    var targetGroupedItems = ((ObservableList<ObservableGrouping<object, object>>)InternalListView.ItemsSource).Select(i => i.Key).ToList();

                    var tempGroupedItemsToUpdate = sourceGroupedItems.Keys.Intersect(targetGroupedItems).ToList();
                    foreach (var groupKey in tempGroupedItemsToUpdate)
                    {
                        var sourceGroup = sourceGroupedItems[groupKey];

                        var targetGroupToUpdate = ((ObservableList<ObservableGrouping<object, object>>)InternalListView.ItemsSource).FirstOrDefault(i => i.Key == groupKey);
                        targetGroupToUpdate.ReplaceRange(sourceGroup);
                        sourceGroupedItems[groupKey] = targetGroupToUpdate;
                    }

                    ((ObservableList<ObservableGrouping<object, object>>)InternalListView.ItemsSource).ReplaceRange(sourceGroupedItems.Values);
                }
                else
                {
                    ((ObservableList<object>)InternalListView.ItemsSource).ReplaceRange(filteredItems);
                }

                if (IsSorted)
                {
                    if (IsGrouped)
                    {
                        foreach(var group in ((ObservableList<ObservableGrouping<object, object>>)InternalListView.ItemsSource))
                        {
                            group.Sort(SortComparer);
                        }
                        ((ObservableList<ObservableGrouping<object, object>>)InternalListView.ItemsSource).Sort(SortComparer);
                    }
                    else
                    {
                        ((ObservableList<object>)InternalListView.ItemsSource).Sort(SortComparer);
                    }
                }
            }
        }

        object GetPropertyValue(object src, string propertyName)
        {
            return src.GetType().GetRuntimeProperty(propertyName)?.GetValue(src);
        }
    }
}
