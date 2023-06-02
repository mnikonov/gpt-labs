namespace Gpt.Labs.ViewModels.Collections.Interfaces
{
    public interface IListCollectionItem
    {
        bool IsInVisibleSubset { get; set; }

        int VisibleSubsetPosition { get; set; }
    }
}
