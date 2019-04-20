using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace BudgetBadger.Models
{
    public class ObservableList<T> : ObservableCollection<T>
    {
        public ObservableList() : base()
        {
        }

        public ObservableList(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
        {
            if (notificationMode != NotifyCollectionChangedAction.Add && notificationMode != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckReentrancy();

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {
                foreach (var i in collection)
                    Items.Add(i);

                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                return;
            }

            int startIndex = Count;
            var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);
            foreach (var i in changedItems)
                Items.Add(i);


            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startIndex));
        }

        public void RemoveRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Remove)
        {
            if (notificationMode != NotifyCollectionChangedAction.Remove && notificationMode != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("Mode must be either Remove or Reset for RemoveRange.", nameof(notificationMode));
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckReentrancy();

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {

                foreach (var i in collection)
                    Items.Remove(i);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                return;
            }

            var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);
            for (int i = 0; i < changedItems.Count; i++)
            {
                if (!Items.Remove(changedItems[i]))
                {
                    changedItems.RemoveAt(i); //Can't use a foreach because changedItems is intended to be (carefully) modified
                    i--;
                }
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems, -1));
        }

        public void Replace(T item)
        {
            ReplaceRange(new T[] { item });
        }

        public void ReplaceRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            Items.Clear();
            AddRange(collection, NotifyCollectionChangedAction.Reset);
        }

        //public void MergeAndSortRange(IEnumerable<T> collection)
        //{
        //    //remove
        //    var itemsToRemove = Items.Where(item => !collection.Any(item2 => item2.Equals(item))).ToList();
            
        //    foreach(var item in itemsToRemove)
        //    {
        //        Remove(item);
        //    }

        //    //add new
        //    var itemsToAdd = collection.Where(item => !Items.Any(item2 => item2.Equals(item))).ToList();

        //    var sortedList = Items.Union(itemsToAdd).ToList();
        //    sortedList.Sort();

        //    for (int i = 0; i < sortedList.Count; i++)
        //    {
        //        if (!Items.Contains(sortedList[i]))
        //        {
        //            InsertItem(i, sortedList[i]);
        //        }
        //    }
        //}

        public void MergeRange(IEnumerable<T> collection)
        {
            //remove
            // Items where the item doesn't exist in the collection
            var itemsToRemove = Items.Where(item => !collection.Any(item2 => item2.Equals(item)));
            RemoveRange(itemsToRemove);

            //add new
            var itemsToAdd = collection.Where(item => !Items.Any(item2 => item2.Equals(item)));
            AddRange(itemsToAdd);
        }
    }
}
