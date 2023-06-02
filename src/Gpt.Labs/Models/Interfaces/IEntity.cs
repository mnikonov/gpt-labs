namespace Gpt.Labs.Models.Interfaces
{
   public interface IEntity<T>
    {
        #region Properties

        T Id { get; set; }

        #endregion
    }
}
