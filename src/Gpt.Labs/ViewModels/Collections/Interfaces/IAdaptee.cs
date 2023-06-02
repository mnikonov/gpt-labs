using Microsoft.UI.Xaml.Data;
using System.Collections.Generic;

namespace Gpt.Labs.ViewModels.Collections.Interfaces
{
    internal interface IAdaptee<TElement, TUid> : IList<TElement>
        where TElement : class
    {
        #region Public Methods

        TElement ElementAt(int index);

        TElement GetById(TUid id);

        bool Contains(TUid id);


        int IndexOfElement(TUid id);


        void ApplyRangesVisibility(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems);

        ItemIndexRange GetVisibleRange();

        #endregion
    }
}
