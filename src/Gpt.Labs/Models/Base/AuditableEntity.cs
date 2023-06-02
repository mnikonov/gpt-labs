using Gpt.Labs.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Gpt.Labs.Models.Base
{
    public abstract class AuditableEntity : Entity, IAuditableEntity
    {
        #region Fields

        private DateTime createdDate;

        private DateTime? updatedDate;

        #endregion

        #region Properties

        [Required]
        public DateTime CreatedDate
        {
            get => this.createdDate;
            set => this.Set(ref this.createdDate, value);
        }

        public DateTime? UpdatedDate
        {
            get => this.updatedDate;
            set => this.Set(ref this.updatedDate, value);
        }

        #endregion
    }
}
