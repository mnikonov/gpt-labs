using System.Collections.Generic;

namespace Gpt.Labs.ViewModels.Collections.Interfaces
{
    public interface IQueryableDataProvider<TElement, TUid>
        where TElement : class
    {
        bool Contains(TUid id);

        TElement GetById(TUid id);

        int GetCount();

        IEnumerable<TElement> GetInRange(int skip, int take);

        int IndexOf(TElement item);
    }
}
