﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace BudgetBadger.Models
{
    /// <summary> 
	/// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
	/// </summary> 
	/// <typeparam name="T"></typeparam> 
	public class ObservableList<T> : ObservableCollection<T>
    {

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
        /// </summary> 
        public ObservableList()
            : base()
        {
        }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
        /// </summary> 
        /// <param name="collection">collection: The collection from which the elements are copied.</param> 
        /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
        public ObservableList(IEnumerable<T> collection)
            : base(collection)
        {
        }

        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        public void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
        {
            if (notificationMode != NotifyCollectionChangedAction.Add && notificationMode != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (!collection.Any())
                return;

            CheckReentrancy();

            var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);

            var startIndex = Count;

            var itemAdded = false;
            foreach (var item in collection)
            {
                Items.Add(item);
                itemAdded = true;
            }

            if (!itemAdded)
                return;

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {
                RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);

                return;
            }

            RaiseChangeNotificationEvents(
                action: NotifyCollectionChangedAction.Add,
                changedItems: changedItems,
                startingIndex: startIndex);
        }

        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T). NOTE: with notificationMode = Remove, removed items starting index is not set because items are not guaranteed to be consecutive.
        /// </summary> 
        public void RemoveAll()
        {
            if (Items.Count > 0)
            {
                RemoveRange(0, Items.Count);
            }
        }

        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T). NOTE: with notificationMode = Remove, removed items starting index is not set because items are not guaranteed to be consecutive.
        /// </summary> 
        public void RemoveRange(int index, int count, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Remove)
        {
            if (notificationMode != NotifyCollectionChangedAction.Remove && notificationMode != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("Mode must be either Remove or Reset for RemoveRange.", nameof(notificationMode));
            if (index < 0)
                throw new ArgumentNullException(nameof(index));
            if (Items.Count < count)
                throw new ArgumentNullException(nameof(count));

            CheckReentrancy();

            var changedItems = new List<T>();
            for (var i = index; i < count; i++)
            {
                changedItems.Add(Items[i]);
                Items.RemoveAt(i);
            }

            if (changedItems.Count == 0)
            {
                return;
            }

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {
                RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);
                return;
            }

            RaiseChangeNotificationEvents(
                action: NotifyCollectionChangedAction.Remove,
                startingIndex: index,
                changedItems: changedItems);
        }

        /// <summary> 
        /// Clears the current collection and replaces it with the specified item. 
        /// </summary> 
        public void Replace(T item) => ReplaceRange(new T[] { item });

        /// <summary> 
        /// Updates the current collection to match the specified collection. 
        /// </summary> 
        public void ReplaceRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var itemsToAdd = collection.Except(Items).ToList();
            if (itemsToAdd.Any())
            {
                AddRange(itemsToAdd);
            }

            var itemsToRemove = Items.Except(collection).ToList();
            if (itemsToRemove.Any())
            {
                var startIndex = 0;
                for (var i = startIndex; i < Items.Count; i++)
                {
                    if (itemsToRemove.Contains(Items[i]))
                    {
                        continue;
                    }

                    RemoveRange(startIndex, i - 1);
                    startIndex = i + 1;
                }
            }
        }

        private void RaiseChangeNotificationEvents(NotifyCollectionChangedAction action, List<T> changedItems = null, int startingIndex = -1)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

            if (changedItems is null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action));
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems: changedItems, startingIndex: startingIndex));
        }
    }

    public static class ObservableListExtensions
    {
        public static void Sort<T>(this ObservableList<T> observable, IComparer<T> comparer)
        {
            List<T> sorted = observable.ToList();
            if (comparer == null)
            {
                sorted.Sort();
            }
            else
            {
                sorted.Sort(comparer);
            }

            int ptr = 0;
            while (ptr < sorted.Count)
            {
                if (!observable[ptr].Equals(sorted[ptr]))
                {
                    T t = observable[ptr];
                    observable.RemoveAt(ptr);
                    observable.Insert(sorted.IndexOf(t), t);
                }
                else
                {
                    ptr++;
                }
            }
        }
    }
}
