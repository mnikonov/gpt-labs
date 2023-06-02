namespace Gpt.Labs.ViewModels.Collections.Interfaces
{
    public interface IEntityDeleteDataProvider<TUid>
    {
        void Delete(TUid id);
    }
}
