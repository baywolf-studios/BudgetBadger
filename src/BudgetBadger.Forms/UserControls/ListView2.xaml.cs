using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using BudgetBadger.Core.Models;
using BudgetBadger.Core.Utilities;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ListView2 : Grid
    {
        private readonly Action _debouncedUpdateItems;

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

        public static BindableProperty SearchFilterProperty = BindableProperty.Create(nameof(SearchFilter), typeof(Predicate<object>), typeof(ListView2), propertyChanged: UpdateItems);
        public Predicate<object> SearchFilter
        {
            get => (Predicate<object>)GetValue(SearchFilterProperty);
            set => SetValue(SearchFilterProperty, value);
        }

        public static BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(ListView2), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSearchTextChanged);
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
        public IComparer<object> SortComparer
        {
            get => (IComparer<object>)GetValue(SortComparerProperty);
            set => SetValue(SortComparerProperty, value);
        }

        public static BindableProperty RefreshCommandProperty = BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(ListView2));
        public ICommand RefreshCommand
        {
            get => (ICommand)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }

        public static BindableProperty IsBusyProperty = BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(ListView2), defaultBindingMode: BindingMode.OneWay, propertyChanged: UpdateIsBusy);
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

        public ListView2()
        {
            InitializeComponent();
            InternalListView.BindingContext = this;
            InternalListView.ItemsSource = new ObservableList<object>();
            InternalListView.ItemTapped += InternalListView_ItemTapped;
            _debouncedUpdateItems = ActionExtensions.Debounce(UpdateItems, 400);
        }

        private void UpdateMac()
        {
            if (Device.RuntimePlatform != Device.macOS)
            {
                return;
            }

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
            SetRow(InternalListView, 1);
            Children.Add(InternalListView);
            InternalListView.BindingContext = this;
            InternalListView.ItemsSource = new ObservableList<object>();
            InternalListView.ItemTapped += InternalListView_ItemTapped;
            InternalListView.SetBinding(ListView.SelectedItemProperty, new Binding(nameof(SelectedItem)));
            InternalListView.SetBinding(ListView.HasUnevenRowsProperty, new Binding(nameof(HasUnevenRows)));
            InternalListView.SetBinding(ListView.RowHeightProperty, new Binding(nameof(RowHeight)));
            InternalListView.SetBinding(ListView.SeparatorVisibilityProperty, new Binding(nameof(SeparatorVisibility)));
            InternalListView.SetBinding(ListView.SeparatorColorProperty, new Binding(nameof(SeparatorColor)));
            InternalListView.SetBinding(ListView.HorizontalScrollBarVisibilityProperty, new Binding(nameof(HorizontalScrollBarVisibility)));
            InternalListView.SetBinding(ListView.VerticalScrollBarVisibilityProperty, new Binding(nameof(VerticalScrollBarVisibility)));

            UpdateHeader();
            UpdateFooter();
        }

        private void InternalListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ItemTapped?.Invoke(this, e);

            if (SelectionMode == ListViewSelectionMode.Single)
            {
                SelectedItem = SelectedItem == e.Item ? null : e.Item;
            }
        }

        private void UpdateIsBusy()
        {
            if (!IsBusy)
            {
                InternalListView.IsRefreshing = false;
                activityIndicator.IsVisible = false;
            }
            else if (!InternalListView.IsRefreshing)
            {
                activityIndicator.IsVisible = true;
            }
        }

        private static void UpdateIsBusy(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.UpdateIsBusy();
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
            if (Device.RuntimePlatform == Device.macOS)
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

        private static void OnSearchTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView._debouncedUpdateItems();
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
                var filteredItems = Items.Cast<object>().AsParallel();

                // filtering
                if (SearchFilter != null)
                {
                    filteredItems = filteredItems.Where(SearchFilter.Invoke);
                }

                if (IsSorted)
                {
                    filteredItems = filteredItems.OrderBy(f => f, SortComparer);
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

        private static object GetPropertyValue(object src, string propertyName)
        {
            if (src == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                return src.ToString();
            }

            var nameParts = propertyName.Split('.');

            if (nameParts.Length == 1)
            {
                src = src.GetType().GetRuntimeProperty(propertyName)?.GetValue(src);
            }
            else
            {
                foreach (var part in nameParts)
                {
                    if (src == null)
                    {
                        return null;
                    }

                    src = src.GetType().GetRuntimeProperty(part)?.GetValue(src);
                }
            }

            return src;
        }
    }
}
