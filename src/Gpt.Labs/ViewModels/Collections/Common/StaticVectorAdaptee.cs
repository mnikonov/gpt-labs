using Gpt.Labs.ViewModels.Collections.Interfaces;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Gpt.Labs.ViewModels.Collections.Common
{
    public class StaticVectorAdaptee<TElement, TUid> : IAdaptee<TElement, TUid>, IDisposable
        where TElement : class
    {
        #region Fields

        private List<TElement> items;

        private readonly Func<TElement, TUid> idSelector;

        private Action<NotifyCollectionChangedEventArgs> notifyCollectionChanged;

        private Action<string> notifyPropertyChanged;

        private bool disposed;

        #endregion

        #region Constructors

        public StaticVectorAdaptee(
            IEnumerable<TElement> collection,
            Func<TElement, TUid> idSelector,
            Action<NotifyCollectionChangedEventArgs> notifyCollectionChanged,
            Action<string> notifyPropertyChanged)
        {
            this.idSelector = idSelector;

            if (collection != null)
            {
                this.items = new List<TElement>(collection);
            }
            else
            {
                this.items = new List<TElement>();
            }

            this.notifyCollectionChanged = notifyCollectionChanged;
            this.notifyPropertyChanged = notifyPropertyChanged;
        }

        ~StaticVectorAdaptee()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        public int Count => this.items.Count;

        public bool IsReadOnly => false;

        #endregion

        #region Indexers

        public TElement this[int index]
        {
            get => this.items[index];

            set
            {
                this.items[index] = value;

                this.notifyCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, index));
            }
        }

        #endregion

        #region Public Methods

        public void Add(TElement item)
        {
            this.items.Add(item);

            this.notifyCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, this.items.Count - 1));
            this.notifyPropertyChanged("Count");
        }

        public void Clear()
        {
            if (this.items.Count != 0)
            {
                this.items.Clear();

                this.notifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                this.notifyPropertyChanged("Count");
            }
        }

        public bool Contains(TElement item)
        {
            return this.items.Contains(item);
        }

        public void CopyTo(TElement[] array, int arrayIndex)
        {
            this.items.CopyTo(array, arrayIndex);
        }

        public TElement ElementAt(int index)
        {
            return this.items.ElementAt(index);
        }

        public TElement GetById(TUid id)
        {
            for (var i = 0; i < this.items.Count; i++)
            {
                if (Equals(this.idSelector(this.items[i]), id))
                {
                    return this.items[i];
                }
            }

            return default;
        }

        public bool Contains(TUid id)
        {
            for (var i = 0; i < this.items.Count; i++)
            {
                if (Equals(this.idSelector(this.items[i]), id))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        public int IndexOf(TElement item)
        {
            return this.items.IndexOf(item);
        }

        public int IndexOfElement(TUid id)
        {
            var entity = this.GetById(id);

            if (entity == null)
            {
                return -1;
            }

            return this.IndexOf(entity);
        }

        public void Insert(int index, TElement item)
        {
            this.items.Insert(index, item);

            this.notifyCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            this.notifyPropertyChanged("Count");
        }

        public bool Remove(TElement item)
        {
            var index = this.items.IndexOf(item);

            if (index != -1 && this.items.Remove(item))
            {
                this.notifyCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));

                this.notifyPropertyChanged("Count");

                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            var item = this.items.ElementAt(index);
            this.items.RemoveAt(index);

            this.notifyCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));

            this.notifyPropertyChanged("Count");
        }

        public void ApplyRangesVisibility(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i] is IListCollectionItem listItem)
                {
                    if (i >= visibleRange.FirstIndex && i <= visibleRange.LastIndex)
                    {
                        listItem.VisibleSubsetPosition = i - visibleRange.FirstIndex;
                        listItem.IsInVisibleSubset = true;
                    }
                    else
                    {
                        listItem.VisibleSubsetPosition = -1;
                        listItem.IsInVisibleSubset = false;
                    }
                }
            }
        }

        public ItemIndexRange GetVisibleRange()
        {
            var visibleRange = this.items.Where(p => p is IListCollectionItem item && item.IsInVisibleSubset).ToList();

            if (visibleRange.Count > 0)
            {
                return new ItemIndexRange(this.IndexOf(visibleRange.First()), (uint)visibleRange.Count);
            }

            return new ItemIndexRange(0, 0);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion

        #region Private Methods

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.items = null;
            this.notifyCollectionChanged = null;
            this.notifyPropertyChanged = null;

            this.disposed = true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        #endregion
    }
}
