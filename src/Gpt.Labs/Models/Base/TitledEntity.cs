using Gpt.Labs.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Gpt.Labs.Models.Base
{
    public class TitledEntity : AuditableEntity, ITitledEntity
    {
        #region Fields

        protected string title;

        #endregion

        #region Properties

        [Required(ErrorMessage = "The 'Title' field is required")]
        [MaxLength(255, ErrorMessage = "The field 'Title' must be a string with a maximum length of {1}.")]
        public string Title
        {
            get => this.title;
            set => this.Set(ref this.title, value);
        }

        #endregion
    }
}
