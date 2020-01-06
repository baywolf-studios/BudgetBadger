using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using BudgetBadger.Models;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ListView2 : Xamarin.Forms.ListView
    {
        public new static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ListView2), null, propertyChanged: OnItemsSourceChanged);
        public new IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static BindableProperty FilterProperty = BindableProperty.Create(nameof(Filter), typeof(Predicate<object>), typeof(ListView2), propertyChanged: UpdateFilter);
        public Predicate<object> Filter
        {
            get => (Predicate<object>)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public static BindableProperty FilterTextProperty = BindableProperty.Create(nameof(FilterText), typeof(string), typeof(ListView2));
        public string FilterText
        {
            get => (string)GetValue(FilterTextProperty);
            set => SetValue(FilterTextProperty, value);
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

        public static BindableProperty IsSortedProperty = BindableProperty.Create(nameof(IsSorted), typeof(bool), typeof(ListView2), propertyChanged: UpdateSorted);
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

        public ListView2()
        {
            InitializeComponent();
            base.ItemsSource = new ObservableRangeCollection<ObservableGrouping<object, object>>();
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView)
            {
                if (oldValue != newValue)
                {
                    listView.ClearItems();
                    if (oldValue is INotifyCollectionChanged oldCollection)
                    {
                        oldCollection.CollectionChanged -= NotifyCollectionChangedEventHandler;
                    }
                    if (newValue is INotifyCollectionChanged newCollection)
                    {
                        newCollection.CollectionChanged += NotifyCollectionChangedEventHandler;
                    }
                }
            }
        }

        private static void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ListView2 listView)
            {
                listView.UpdateItems();
            }
        }

        private static void UpdateFilter(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.ClearItems();
                listView.UpdateItems();
            }
        }

        private static void UpdateGrouped(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.ClearItems();
                listView.IsGroupingEnabled = listView.IsGrouped;
                listView.UpdateItems();
            }
        }

        private static void UpdateSorted(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView && oldValue != newValue)
            {
                listView.ClearItems();
                listView.UpdateItems();
            }
        }

        private void ClearItems()
        {
            ((ObservableRangeCollection<ObservableGrouping<object, object>>)base.ItemsSource).Clear();
        }

        private void UpdateItems()
        {
            if (ItemsSource == null)
            {
                ClearItems();
            }
            else
            {
                var filteredItems = new List<object>();

                // filtering
                if (Filter != null)
                {
                    foreach (var item in ItemsSource)
                    {
                        if (Filter.Invoke(item))
                        {
                            filteredItems.Add(item);
                        }
                    }
                }
                else
                {
                    var itemSource = ItemsSource?.Cast<object>().ToList();
                    filteredItems.AddRange(itemSource);
                }

                //grouping
                if (IsGrouped)
                {
                    var groupedItems = filteredItems.GroupBy(g => GetPropertyValue(g, GroupPropertyDescription));

                    var sourceGroupedItems = groupedItems.ToDictionary(a => a.Key, a2 => new ObservableGrouping<object, object>(a2.Key, a2));
                    var targetGroupedItems = ((ObservableRangeCollection<ObservableGrouping<object, object>>)base.ItemsSource).Select(i => i.Key);

                    var tempGroupedItemsToUpdate = sourceGroupedItems.Keys.Intersect(targetGroupedItems);
                    foreach (var groupKey in tempGroupedItemsToUpdate)
                    {
                        var sourceGroup = sourceGroupedItems[groupKey];

                        var targetGroupToUpdate = ((ObservableRangeCollection<ObservableGrouping<object, object>>)base.ItemsSource).FirstOrDefault(i => i.Key == groupKey);
                        targetGroupToUpdate.ReplaceRange(sourceGroup);
                        sourceGroupedItems[groupKey] = targetGroupToUpdate;
                    }

                    ((ObservableRangeCollection<ObservableGrouping<object, object>>)base.ItemsSource).ReplaceRange(sourceGroupedItems.Values);
                }

                if (IsSorted)
                {
                    ((ObservableRangeCollection<ObservableGrouping<object, object>>)base.ItemsSource).Sort(SortComparer);
                }
            }
        }

        object GetPropertyValue(object src, string propertyName)
        {
            return src.GetType().GetRuntimeProperty(propertyName)?.GetValue(src);
        }
    }
}
