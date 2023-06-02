using Gpt.Labs.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Gpt.Labs.Models.Base
{
    public abstract class Entity : ObservableValidationObject, IEntity<Guid>
    {
        #region Fields

        private Guid id;

        #endregion

        #region Properties

        [Key]
        public Guid Id
        {
            get => this.id;
            set
            {
                if (this.Set(ref this.id, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsNew));
                }
            }
        }

        [JsonIgnore]
        [NotMapped]
        public bool IsNew => this.id == default;

        #endregion
    }
}
