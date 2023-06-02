using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.ViewModels.Collections.Interfaces;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;

namespace Gpt.Labs.ViewModels.Collections.Common
{
    public class QueryableVectorAdaptee<TElement, TUid> : IPagedAdaptee<TElement, TUid>, IDisposable
        where TElement : class
    {
        #region Fields

        public const int PageSize = 50;

        internal readonly IQueryableDataProvider<TElement, TUid> Provider;

        private readonly Func<TElement, TUid> idSelector;

        private Action<NotifyCollectionChangedEventArgs> notifyCollectionChanged;

        private Action<string> notifyPropertyChanged;

        private bool disposed;

        #endregion

        #region Constructors

        public QueryableVectorAdaptee(
            IQueryableDataProvider<TElement, TUid> provider,
            Func<TElement, TUid> idSelector,
            Action<NotifyCollectionChangedEventArgs> notifyCollectionChanged,
            Action<string> notifyPropertyChanged)
        {
            this.idSelector = idSelector;

            this.Provider = provider;
            this.notifyCollectionChanged = notifyCollectionChanged;
            this.notifyPropertyChanged = notifyPropertyChanged;
        }

        ~QueryableVectorAdaptee()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        public int Count { get; set; } = -1;

        public bool IsReadOnly => true;

        public ConcurrentDictionary<int, Page<TElement>> Pages { get; } = new ConcurrentDictionary<int, Page<TElement>>();

        #endregion

        #region Indexers

        public TElement this[int index]
        {
            get
            {
                if (index < 0 || index > this.Count)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        "Requested index is out of range inside QueryableVectorAdaptee");
                }

                var pageIndex = index / PageSize;

                this.AddOrUpdatePage(pageIndex);

                var pageOffset = index % PageSize;

                if (!this.Pages.ContainsKey(pageIndex) || this.Pages[pageIndex]?.Items == null
                                                       || this.Pages[pageIndex]?.Items.Length <= pageOffset)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        "Unexpected exception on attempt to get value from QueryableVectorAdaptee by index");
                }

                return this.Pages[pageIndex].Items[pageOffset];
            }

            set
            {
                if (index < 0 || index > this.Count)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        "Requested index is out of range inside QueryableVectorAdaptee");
                }

                var pageIndex = index / PageSize;

                this.AddOrUpdatePage(pageIndex);

                var pageOffset = index % PageSize;

                if (!this.Pages.ContainsKey(pageIndex) || this.Pages[pageIndex]?.Items == null
                                                       || this.Pages[pageIndex]?.Items.Length <= pageOffset)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        "Unexpected exception on attempt to set value from QueryableVectorAdaptee by index");
                }

                this.Pages[pageIndex].Items[pageOffset] = value;
            }
        }

        #endregion

        #region Public Methods

        public void Add(TElement item)
        {
            if (item != null)
            {
                var index = this.IndexOf(item);

                if (index != -1)
                {
                    this.Insert(index, item);
                }
            }
        }

        public void Clear()
        {
            if (this.Count != 0)
            {
                this.Count = 0;

                this.Pages.Clear();

                this.notifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                this.notifyPropertyChanged("Count");
            }
        }

        public bool Contains(TElement item)
        {
            foreach (var index in this.Pages.Keys)
            {
                if (!this.Pages.TryGetValue(index, out var page) || page?.Items == null)
                {
                    continue;
                }

                for (var i = 0; i < page.Items.Length; i++)
                {
                    if (Equals(this.idSelector(page.Items[i]), this.idSelector(item)))
                    {
                        return true;
                    }
                }
            }

            return this.Provider.Contains(this.idSelector(item));
        }

        public bool Contains(TUid id)
        {
            foreach (var index in this.Pages.Keys)
            {
                if (!this.Pages.TryGetValue(index, out var page) || page?.Items == null)
                {
                    continue;
                }

                for (var i = 0; i < page.Items.Length; i++)
                {
                    if (Equals(this.idSelector(page.Items[i]), id))
                    {
                        return true;
                    }
                }
            }

            return this.Provider.Contains(id);
        }

        public void CopyTo(TElement[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public TElement ElementAt(int index)
        {
            return this[index];
        }

        public TElement GetById(TUid id)
        {
            foreach (var index in this.Pages.Keys)
            {
                if (!this.Pages.TryGetValue(index, out var page) || page?.Items == null)
                {
                    continue;
                }

                for (var i = 0; i < page.Items.Length; i++)
                {
                    if (Equals(this.idSelector(page.Items[i]), id))
                    {
                        return page.Items[i];
                    }
                }
            }

            var elementIndex = this.IndexOfElement(id);

            if (elementIndex >= 0)
            {
                var element = this.ElementAt(elementIndex);

                var pageIndex = elementIndex / PageSize;

                if (this.Pages.ContainsKey(pageIndex) && this.Pages[pageIndex].Items != null)
                {
                    for (var i = 0; i < this.Pages[pageIndex].Items.Length; i++)
                    {
                        var index = (pageIndex * PageSize) + i;
                        this.notifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, this.ElementAt(index), this.ElementAt(index), index));
                    }
                }

                return element;
            }

            return null;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            var elements = new List<TElement>();

            foreach (var index in this.Pages.Keys)
            {
                if (!this.Pages.TryGetValue(index, out var page) || page?.Items == null)
                {
                    continue;
                }

                foreach (var item in page.Items)
                {
                    elements.Add(item);
                }
            }

            return elements.GetEnumerator();
        }

        public int IndexOf(TElement item)
        {
            if (item == null)
            {
                return -1;
            }

            var itemId = this.idSelector(item);

            foreach (var pageNumber in this.Pages.Keys)
            {
                if (!this.Pages.TryGetValue(pageNumber, out var page) || page?.Items == null)
                {
                    continue;
                }

                for (var i = 0; i < page.Items.Length; i++)
                {
                    if (Equals(this.idSelector(page.Items[i]), itemId))
                    {
                        return (pageNumber * PageSize) + i;
                    }
                }
            }

            if (!this.Provider.Contains(itemId))
            {
                return -1;
            }

            return this.Provider.IndexOf(item);

            // var index = this.provider.IndexOf(item);

            // if (index == -1)
            // {
            // return index;
            // }

            // var pageIndex = index / this.PageSize;

            // this.AddOrUpdatePage(pageIndex);

            // if (this.Pages.ContainsKey(pageIndex) && this.Pages[pageIndex].Items != null)
            // {
            // for (var i = 0; i < this.Pages[pageIndex].Items.Length; i++)
            // {
            // this.notifyVectorChanged(new VectorChangedEventArgs { CollectionChange = CollectionChange.ItemChanged, Index = (uint)((pageIndex * this.PageSize) + i) });
            // }

            // this.notifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            // }

            // return index;
        }

        public int IndexOfElement(TUid id)
        {
            foreach (var index in this.Pages.Keys)
            {
                if (!this.Pages.TryGetValue(index, out var page) || page?.Items == null)
                {
                    continue;
                }

                for (var i = 0; i < page.Items.Length; i++)
                {
                    if (Equals(this.idSelector(page.Items[i]), id))
                    {
                        return (index * PageSize) + i;
                    }
                }
            }

            var entity = this.Provider.GetById(id);

            if (entity == null)
            {
                return -1;
            }

            return this.Provider.IndexOf(entity);
        }

        public void InitElementPage(int elementIndex)
        {
            this.Count = this.Provider.GetCount();
            this.AddOrUpdatePage(elementIndex / PageSize);
        }


        public void Insert(int index, TElement item)
        {
            // (this.provider as IEntityDeleteDataProvider)?.Delete((int)this.idSelector(item));
            var pageIndex = index / PageSize;

            this.Count += 1;

            foreach (var key in this.Pages.Keys.ToList())
            {
                if (key >= pageIndex)
                {
                    this.Pages.TryRemove(key, out var _);
                }
            }

            this.AddOrUpdatePage(pageIndex);

            var element = this.ElementAt(index);

            this.notifyCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element, index));

            this.notifyPropertyChanged("Count");
        }

        public void Refresh()
        {
            var oldCount = this.Count;

            this.Count = this.Provider.GetCount();

            this.Pages.Clear();

            this.AddOrUpdatePage(0);

            try
            {
                this.notifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                if (oldCount != this.Count)
                {
                    this.notifyPropertyChanged("Count");
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // NOTE: Sometimes NotifyCollectionChanged fails with ArgumentOutOfRangeException with message 
                // This collection cannot work with indices larger than Int32.MaxValue - 1 (0x7FFFFFFF - 1).
                // Parameter name: index
                // to reproduce issue search Home page news with 1, than 12 ... last 1234 
                // UI updates correctly
            }
        }

        public bool Remove(TElement item)
        {
            if (item == null)
            {
                return false;
            }

            var index = this.IndexOf(item);

            if (index != -1)
            {
                return this.Remove(index, item);
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (index == -1)
            {
                return;
            }

            var item = this.ElementAt(index);

            if (item != null)
            {
                this.Remove(index, item);
            }
        }

        public void ApplyRangesVisibility(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
            var trackedPages = new HashSet<int>();
            var visiblePages = new HashSet<int>();

            foreach (var item in trackedItems)
            {
                for (var i = item.FirstIndex; i <= item.LastIndex; i++)
                {
                    var pageIndex = i / PageSize;

                    if (!trackedPages.Contains(pageIndex))
                    {
                        trackedPages.Add(pageIndex);
                    }

                    i = (pageIndex * PageSize) + PageSize;
                }
            }

            for (var i = visibleRange.FirstIndex; i <= visibleRange.LastIndex; i++)
            {
                var pageIndex = i / PageSize;

                if (!visiblePages.Contains(pageIndex))
                {
                    visiblePages.Add(pageIndex);
                }

                i = (pageIndex * PageSize) + PageSize;
            }

            foreach (var key in this.Pages.Keys.ToList())
            {
                if (this.Pages.TryGetValue(key, out var page))
                {
                    page.IsVisible = visiblePages.Contains(key);

                    for (var i = 0; i < page.Items.Length; i++)
                    {
                        if (page.Items[i] is IListCollectionItem listItem)
                        {
                            var collectionIndex = (key * PageSize) + i;

                            if (collectionIndex >= visibleRange.FirstIndex && collectionIndex <= visibleRange.LastIndex)
                            {
                                listItem.VisibleSubsetPosition = collectionIndex - visibleRange.FirstIndex;
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
            }
        }

        public ItemIndexRange GetVisibleRange()
        {
            int minPage = -1;
            int minPageFirstIndex = 0;
            int maxPage = 0;
            int maxPageItemsCount = 0;

            foreach (var key in this.Pages.Keys.ToList())
            {
                if (this.Pages.TryGetValue(key, out var page) && page.IsVisible)
                {
                    if (minPage == -1)
                    {
                        minPage = key;
                        var visibleItem = page.Items.First(p => p is IListCollectionItem item && item.IsInVisibleSubset);
                        minPageFirstIndex = Array.IndexOf(page.Items, visibleItem);
                    }

                    if (maxPage <= key)
                    {
                        maxPage = key;

                        maxPageItemsCount = page.Items.Count(p => p is IListCollectionItem item && item.IsInVisibleSubset);
                    }
                }
            }
            
            return new ItemIndexRange((minPage * PageSize) + minPageFirstIndex, (uint)((maxPage * PageSize) + maxPageItemsCount));
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

            this.Pages.Clear();
            this.notifyCollectionChanged = null;
            this.notifyPropertyChanged = null;

            this.disposed = true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void AddOrUpdatePage(int pageIndex)
        {
            // Debug.WriteLine("Page index " + pageIndex);
            try
            {
                this.Pages.AddOrUpdate(
                    pageIndex,
                    n =>
                        {
                            var p = new Page<TElement>(n)
                            {
                                Items = this.Provider.GetInRange(n * PageSize, PageSize).ToArray()
                            };

                            // Debug.WriteLine("Page INSERTED: " + pageIndex + "   for collection: " + this.collectionId);
                            return p;
                        },
                    (n, p) =>
                        {
                            // Debug.WriteLine("Page UPDATED: " + p.PageNumber + "   for collection: " + this.collectionId);
                            return p;
                        });
            }
            catch (Exception ex)
            {
                ex.LogError("Unable to add or update page in QueryableVectorAdaptee");
            }
        }

        private bool Remove(int index, TElement item)
        {
            (this.Provider as IEntityDeleteDataProvider<TUid>)?.Delete(this.idSelector(item));

            var pageIndex = index / PageSize;

            this.Count -= 1;

            foreach (var key in this.Pages.Keys.ToList())
            {
                if (key >= pageIndex)
                {
                    this.Pages.TryRemove(key, out var _);
                }
            }

            if (this.Count > 0)
            {
                this.AddOrUpdatePage(pageIndex);
            }

            this.notifyCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));

            this.notifyPropertyChanged("Count");

            return true;
        }

        #endregion
    }
}
