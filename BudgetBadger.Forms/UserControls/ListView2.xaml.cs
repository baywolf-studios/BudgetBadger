using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using BudgetBadger.Models;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ListView2 : Xamarin.Forms.ListView
    {
        readonly ObservableRangeCollection<ObservableGrouping<object, object>> internalItems;

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
            internalItems = new ObservableRangeCollection<ObservableGrouping<object, object>>();
        }

        private static void UpdateFilter(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView)
            {
                listView.internalItems.Clear();
                listView.UpdateItems();
            }
        }

        private static void UpdateGrouped(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView)
            {
                listView.internalItems.Clear();
                listView.UpdateItems();
            }
        }

        private static void UpdateSorted(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListView2 listView)
            {
                listView.internalItems.Clear();
                listView.UpdateItems();
            }
        }

        private void UpdateItems()
        {
            var filteredItems = new List<object>();

            // filtering
            if (Filter != null)
            {
                foreach (var item in internalItems)
                {
                    if (Filter.Invoke(item))
                    {
                        filteredItems.Add(item);
                    }
                }
            }
            else
            {
                filteredItems.AddRange(internalItems);
            }

            //grouping
            if (IsGrouped)
            {
                var groupedItems = filteredItems.GroupBy(g => GetPropertyValue(g, GroupPropertyDescription));

                //merge each groupedItems into the internalList grouped item

                //add any new groupedItems that don't already exist in the internalList

                //remove any groups in the internalList if they don't exist in the groupedItems

                var sourceGroupedItems = groupedItems.ToDictionary(a => a.Key, a2 => a2);
                var targetGroupedItems = internalItems.ToDictionary(a => a.Key, a2 => a2);

                //var groupedItemsToAdd = sourceGroupedItems.Keys.Except(targetGroupedItems.Keys);
                //foreach (var groupKey in groupedItemsToAdd)
                //{
                //    var groupedItemToAdd = sourceGroupedItems[groupKey];
                //    internalItems.Add(groupedItemToAdd);
                //}

                //var accountsToUpdate = sourceAccountsDictionary.Keys.Intersect(targetAccountsDictionary.Keys);
                //foreach (var accountId in accountsToUpdate)
                //{
                //    var sourceAccount = sourceAccountsDictionary[accountId];
                //    var targetAccount = targetAccountsDictionary[accountId];

                //    if (sourceAccount.ModifiedDateTime > targetAccount.ModifiedDateTime)
                //    {
                //        await targetAccountDataAccess.UpdateAccountAsync(sourceAccount);
                //    }
                //}

                foreach (var group in groupedItems)
                {
                    //if (internalItems.Contains.)
                }
            }

            if (IsSorted)
            {
                internalItems.Sort(SortComparer);
            }
            else
            {

            }
        }

        object GetPropertyValue(object src, string propertyName)
        {
            return src.GetType().GetRuntimeProperty(propertyName)?.GetValue(src);
        }
    }
}
