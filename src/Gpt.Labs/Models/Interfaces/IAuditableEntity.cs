using System;

namespace Gpt.Labs.Models.Interfaces
{
    public interface IAuditableEntity
    {
        DateTime CreatedDate { get; set; }

        DateTime? UpdatedDate { get; set; }
    }
}
