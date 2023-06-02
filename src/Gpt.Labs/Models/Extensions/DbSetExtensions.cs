using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Gpt.Labs.Models.Extensions
{
    public static class DbSetExtensions
    {
        #region Public Methods

        public static DataContext GetDataContext<TEntity>(this DbSet<TEntity> set)
            where TEntity : class
        {
            return set.GetService<ICurrentDbContext>().Context as DataContext;
        }

        #endregion
    }
}
