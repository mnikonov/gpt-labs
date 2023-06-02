namespace Gpt.Labs.ViewModels.Collections.Interfaces
{
    interface IPagedAdaptee<TElement, TUid> : IAdaptee<TElement, TUid>
        where TElement : class
    {
        void InitElementPage(int elementIndex);

        void Refresh();
    }
}
