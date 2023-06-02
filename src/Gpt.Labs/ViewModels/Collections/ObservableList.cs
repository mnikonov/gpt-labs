using Gpt.Labs.Models;
using Gpt.Labs.ViewModels.Collections.Common;
using Gpt.Labs.ViewModels.Collections.Interfaces;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Gpt.Labs.ViewModels.Collections
{
    public class ObservableList<TElement, TUid> : IItemsRangeInfo, INotifyCollectionChanged, INotifyPropertyChanged, IList, IRefresh, ICollection<TElement>
        where TElement : class
    {
        #region Fields

        private IAdaptee<TElement, TUid> adaptee;

        private bool disposed = false;

        #endregion

        #region Constructors and Destructors

        public ObservableList(IQueryableDataProvider<TElement, TUid> provider, Func<TElement, TUid> idSelector)
        {
            this.adaptee = new QueryableVectorAdaptee<TElement, TUid>(provider, idSelector, this.RaiseCollectionChanged, this.RaisePropertyChanged);
        }

        public ObservableList(Func<TElement, TUid> idSelector)
        {
            this.adaptee = new StaticVectorAdaptee<TElement, TUid>(null, idSelector, this.RaiseCollectionChanged, this.RaisePropertyChanged);
        }

        public ObservableList(IEnumerable<TElement> items, Func<TElement, TUid> idSelector)
        {
            this.adaptee = new StaticVectorAdaptee<TElement, TUid>(items, idSelector, this.RaiseCollectionChanged, this.RaisePropertyChanged);
        }

        ~ObservableList()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Events

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Explicit Interface Properties

        public int Count => this.adaptee.Count;

        #endregion

        #region Explicit Interface Indexers

        public TElement this[int index]
        {
            get
            {
                return this.adaptee[index];
            }

            set
            {
                this.adaptee[index] = value;
            }
        }

        #endregion

        #region Public Methods

        public void Initialize(int elementIndex)
        {
            (this.adaptee as IPagedAdaptee<TElement, TUid>)?.InitElementPage(elementIndex);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
            this.adaptee.ApplyRangesVisibility(visibleRange, trackedItems);
        }

        public ItemIndexRange GetVisibleRange()
        {
            return this.adaptee.GetVisibleRange();
        }

        public void Refresh()
        {
            (this.adaptee as IPagedAdaptee<TElement, TUid>)?.Refresh();
        }

        #endregion

        #region Explicit Interface Methods

        public void Clear()
        {
            this.adaptee.Clear();
        }

        public void Add(TElement item)
        {
            this.adaptee.Add(item);
        }

        public bool Contains(TElement item)
        {
            return this.adaptee.Contains(item);
        }

        public bool Contains(TUid id)
        {
            return this.adaptee.Contains(id);
        }

        public void CopyTo([WriteOnlyArray] TElement[] array, int arrayIndex)
        {
            this.adaptee.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.adaptee.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.adaptee).GetEnumerator();
        }

        public TElement GetById(TUid id)
        {
            return this.adaptee.GetById(id);
        }

        public int IndexOf(TElement item)
        {
            return this.adaptee.IndexOf(item);
        }

        public TElement ElementAt(int index)
        {
            return this.adaptee.ElementAt(index);
        }

        public int IndexOfElement(TUid id)
        {
            return this.adaptee.IndexOfElement(id);
        }

        public void Insert(int index, TElement item)
        {
            this.adaptee.Insert(index, item);
        }

        public bool Remove(TElement item)
        {
            return this.adaptee.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.adaptee.RemoveAt(index);
        }

        #endregion

        #region Protected Methods

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            this.CollectionChanged?.Invoke(this, args);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                (this.adaptee as IDisposable)?.Dispose();
                this.adaptee = null;
            }

            this.disposed = true;
        }

        #endregion

        #region IList implementation

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => false;

        int ICollection.Count => this.adaptee.Count;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => new object();

        bool ICollection<TElement>.IsReadOnly => false;

        object IList.this[int index]
        {
            get
            {
                return this.adaptee[index];
            }

            set
            {
                this.adaptee[index] = (TElement)value;
            }
        }

        int IList.Add(object value)
        {
            this.adaptee.Add((TElement)value);
            return this.adaptee.Count - 1;
        }

        void IList.Clear()
        {
            this.adaptee.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.adaptee.Contains((TElement)value);
        }

        int IList.IndexOf(object value)
        {
            return this.adaptee.IndexOf((TElement)value);
        }

        void IList.Insert(int index, object value)
        {
            this.adaptee.Insert(index, (TElement)value);
        }

        void IList.Remove(object value)
        {
            this.adaptee.Remove((TElement)value);
        }

        void IList.RemoveAt(int index)
        {
            this.adaptee.RemoveAt(index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
