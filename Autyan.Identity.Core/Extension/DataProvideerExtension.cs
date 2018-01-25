using System.Linq;
using System.Threading.Tasks;
using Autyan.Identity.Core.Data;
using Autyan.Identity.Core.DataProvider;

namespace Autyan.Identity.Core.Extension
{
    public static class DataProvideerExtension
    {
        public static TEntity FirstOrDefault<TEntity, TQuery>(this IDataProvider<TEntity, TQuery> dataprovider, object query)
            where TEntity : BaseEntity
            where TQuery : BaseQuery<TEntity>
        {
            return dataprovider.Where(query).FirstOrDefault();
        }

        public static async Task<TEntity> FirstOrDefaultAsync<TEntity, TQuery>(this IDataProvider<TEntity, TQuery> dataprovider, object query)
            where TEntity : BaseEntity
            where TQuery : BaseQuery<TEntity>
        {
            var queryResult = await dataprovider.WhereAsync(query);
            return queryResult.FirstOrDefault();
        }
    }
}
