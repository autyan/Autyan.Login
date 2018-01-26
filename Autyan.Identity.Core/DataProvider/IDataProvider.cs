using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autyan.Identity.Core.Data;

namespace Autyan.Identity.Core.DataProvider
{
    public interface IDataProvider<TEntity, TQuery> : IDisposable
        where TEntity : BaseEntity
        where TQuery : BaseQuery<TEntity>
    {
        Task<IEnumerable<TEntity>> QueryAsync(TQuery query);

        Task<int> DeleteByIdAsync(long? id);

        Task<int> UpdateByIdAsync(TEntity entity);

        Task<PagedResult<TEntity>> PagingQueryAsync(TQuery query);

        Task<long> InsertAsync(TEntity entity);

        Task<int> GetCountAsync(object condition);
    }
}