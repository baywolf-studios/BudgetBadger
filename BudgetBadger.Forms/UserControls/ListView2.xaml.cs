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
    public partial class ListView2 : Grid
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

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(ListView), null, BindingMode.TwoWay, propertyChanged: UpdateSelectedItem);
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

        public static readonly BindableProperty HideSearchBarProperty = BindableProperty.Create(nameof(HideSearchBar), typeof(bool), typeof(ListView2), false, propertyChanged: UpdateHideSearchBar);
        public bool HideSearchBar
        {
            get { return (bool)GetValue(HideSearchBarProperty); }
            set { SetValue(HideSearchBarProperty, value); }
        }

        public static BindableProperty SearchFilterProperty = BindableProperty.Create(nameof(SearchFilter), typeof(Predicate<object>), typeof(ListView2), propertyChanged: UpdateItems);
        public Predicate<object> SearchFilter
        {
            get => (Predicate<object>)GetValue(SearchFilterProperty);
            set => SetValue(SearchFilterProperty, value);
        }

        public static BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(ListView2), defaultBindingMode: BindingMode.TwoWay, propertyChanged: UpdateItems);
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

        public static BindableProperty RefreshCommandProperty = BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(ListView2));
        public ICommand RefreshCommand
        {
            get => (ICommand)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }

        public static BindableProperty IsBusyProperty = BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(ListView2), defaultBindingMode: BindingMode.OneWay);
        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public static BindableProperty IsPullToRefreshEnabledProperty = BindableProperty.Create(nameof(IsPullToRefreshEnabled), typeof(bool), typeof(ListView2), defaultBindingMode: BindingMode.OneWay);
        public bool IsPullToRefreshEnabled
        {
            get => (bool)GetValue(IsPullToRefreshEnabledProperty);
            set => SetValue(IsPullToRefreshEnabledProperty, value);
        }

        public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;
        public event EventHandler<ItemTappedEventArgs> ItemTapped;

        private double previousY;
        private double minY;
        private double maxY;
        private bool atStart = true;
        private bool atEnd;
        private bool scrollingToStart;
        private DateTime lastSearchBarChange = DateTime.Now;

        public ListView2()
        {
            InitializeComponent();
            SearchBar.BindingContext = this;
            InternalListView.BindingContext = this;
            InternalListView.ItemsSource = new ObservableList<object>();
            InternalListView.ItemTapped += InternalListView_ItemTapped;
            InternalListView.Scrolled += InternalListView_Scrolled;
        }

        private void UpdateMac()
        {
            if (Device.RuntimePlatform == Device.macOS)
            {
                if (IsGrouped)
                {
                    InternalListView = new Xamarin.Forms.ListView(ListViewCachingStrategy.RetainElement)
                    {
                        SelectionMode = ListViewSelectionMode.None
                    };
                }
                else
                {
                    InternalListView = new Xamarin.Forms.ListView(ListViewCachingStrategy.RecycleElement)
                    {
                        SelectionMode = ListViewSelectionMode.None
                    };
                }
                SetRow(InternalListView, 2);
                Children.Add(InternalListView);
                InternalListView.BindingContext = this;
                InternalListView.ItemsSource = new ObservableList<object>();
                InternalListView.ItemTapped += InternalListView_ItemTapped;
                InternalListView.Scrolled += InternalListView_Scrolled;
                InternalListView.SetBinding(SelectedItemProperty, nameof(SelectedItem));
                InternalListView.SetBinding(HasUnevenRowsProperty, nameof(HasUnevenRows));
                InternalListView.SetBinding(RowHeightProperty, nameof(RowHeight));
                InternalListView.SetBinding(SeparatorVisibilityProperty, nameof(SeparatorVisibility));
                InternalListView.SetBinding(SeparatorColorProperty, nameof(SeparatorColor));
                InternalListView.SetBinding(HorizontalScrollBarVisibilityProperty, nameof(HorizontalScrollBarVisibility));
                InternalListView.SetBinding(VerticalScrollBarVisibilityProperty, nameof(VerticalScrollBarVisibility));

                UpdateHeader();
                UpdateFooter();
                UpdateHideSearchBar();
            }
        }

        private void InternalListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ItemTapped?.Invoke(this, e);
            
            if (SelectionMode == ListViewSelectionMode.Single)
            {
                if (SelectedItem == e.Item)
                {
                    SelectedItem = null;
                }
                else
                {
                    SelectedItem = e.Item;
                }
            }
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
                minY = Math.Max(e.ScrollY, 0);
            }

            //check if it's close enough to the end
            if (e.ScrollY / maxY > 0.9)
            {
                atEnd = true;
            }
            else
            {
                atEnd = false;
            }

            //check if it's close enough to the beginning
            if (e.ScrollY / maxY < 0.5)
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
            previousY = (previousY + e.ScrollY) / 2;

            if (!HideSearchBar)
            {
                var newSearchBarIsVisible = !atEnd && (atStart || scrollingToStart);
                var shouldChange = (SearchBar.IsVisible != newSearchBarIsVisible && lastSearchBarChange < DateTime.Now);

                if (shouldChange)
                {
                    SearchBar.IsVisible = newSearchBarIsVisible;
                    lastSearchBarChange = DateTime.Now.AddMilliseconds(250);
                }
            }
        }

        private void UpdateHideSearchBar()
        {
            SearchBar.IsVisible = !HideSearchBar;
        }
        private static void UpdateHideSearchBar(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.UpdateHideSearchBar();
            }
        }

        private void UpdateHeader()
        {
            if (IsHeaderSticky)
            {
                InternalListView.Header = null;
                InternalListView.HeaderTemplate = null;

                if (HeaderTemplate != null)
                {
                    var headerView = (View)HeaderTemplate.CreateContent();
                    headerView.BindingContext = Header;
                    stickyHeader.Content = headerView;
                }
                else
                {
                    stickyHeader.Content = (View)Header;
                }

                stickyHeader.IsVisible = true;
            }
            else
            {
                stickyHeader.IsVisible = false;
                InternalListView.Header = Header;
                InternalListView.HeaderTemplate = HeaderTemplate;
            }

        }
        private static void UpdateHeader(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.UpdateHeader();
            }
        }

        private void UpdateFooter()
        {
            if (IsFooterSticky || Device.RuntimePlatform == Device.macOS)
            {
                InternalListView.Footer = null;
                InternalListView.FooterTemplate = null;

                if (FooterTemplate != null)
                {
                    var footerView = (View)FooterTemplate.CreateContent();
                    footerView.BindingContext = Footer;
                    stickyFooter.Content = footerView;
                }
                else
                {
                    stickyFooter.Content = (View)Footer;
                }

                stickyFooter.IsVisible = true;
            }
            else
            {
                stickyFooter.IsVisible = false;
                InternalListView.Footer = Footer;
                InternalListView.FooterTemplate = FooterTemplate;
            }
        }
        private static void UpdateFooter(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.UpdateFooter();
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
                listView.UpdateMac();
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

        private void UpdateSelectedItem()
        {
            if (InternalListView.SelectedItem != SelectedItem)
            {
                InternalListView.SelectedItem = SelectedItem;

                var selectedArgs = new SelectedItemChangedEventArgs(SelectedItem, -1);
                ItemSelected?.Invoke(this, selectedArgs);

                if (SelectedCommand != null && SelectedCommand.CanExecute(SelectedItem))
                {
                    SelectedCommand.Execute(SelectedItem);
                }
            }
        }
        private static void UpdateSelectedItem(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.UpdateSelectedItem();
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
                    ((ObservableList<ObservableGrouping<object, object>>)InternalListView.ItemsSource).ReplaceRange(new ObservableList<ObservableGrouping<object, object>>());
                }
                else
                {
                    ((ObservableList<object>)InternalListView.ItemsSource).ReplaceRange(new ObservableList<object>());
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

                if (IsSorted)
                {
                    filteredItems.Sort(SortComparer);
                }

                //grouping
                if (IsGrouped)
                {
                    var groupedItems = filteredItems.GroupBy(g => GetPropertyValue(g, GroupPropertyDescription));

                    var sourceGroupedItems = groupedItems.Select(a2 => new ObservableGrouping<object, object>(a2.Key, a2));

                    ((ObservableList<ObservableGrouping<object, object>>)InternalListView.ItemsSource).ReplaceRange(sourceGroupedItems);
                }
                else
                {
                    ((ObservableList<object>)InternalListView.ItemsSource).ReplaceRange(filteredItems);
                }
            }
        }

        object GetPropertyValue(object src, string propertyName)
        {
            //return src.GetType().GetRuntimeProperty(propertyName)?.GetValue(src);

            string[] nameParts = propertyName.Split('.');

            if (nameParts.Length == 1)
            {
                return src.GetType().GetRuntimeProperty(propertyName)?.GetValue(src);
            }

            foreach (var part in nameParts)
            {
                if (src == null) { return null; }

                Type type = src.GetType();

                PropertyInfo info = type.GetRuntimeProperty(part);

                if (info == null)
                { return null; }

                src = info.GetValue(src, null);
            }
            return src;
        }
    }
}
