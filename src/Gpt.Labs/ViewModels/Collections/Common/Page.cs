namespace Gpt.Labs.ViewModels.Collections.Common
{
    public class Page<T> where T : class
    {
        #region Constructors

        public Page(int pageNumber)
        {
            this.PageNumber = pageNumber;
            this.Items = null;
        }

        #endregion
        
        #region Public Properties

        public T[] Items { get; set; }

        public int PageNumber { get; set; }
        
        public bool IsVisible { get; set; }

        #endregion
    }
}
