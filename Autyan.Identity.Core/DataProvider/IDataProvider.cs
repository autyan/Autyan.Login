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
        IEnumerable<TEntity> Where(object query);

        Task<IEnumerable<TEntity>> WhereAsync(object query);

        int DeleteById(TEntity entity);

        Task<int> DeleteByIdAsync(TEntity entity);

        int DeleteByCondition(object condition);

        Task<int> DeleteByConditionAsync(object condition);

        int UpdateById(TEntity entity);

        Task<int> UpdateByIdAsync(TEntity entity);

        int UpdateByCondition(object data, object condition);

        Task<int> UpdateByConditionAsync(object data, object condition);

        PagedResult<TEntity> PagingQuery(TQuery query);

        Task<PagedResult<TEntity>> PagingQueryAsync(TQuery query);

        long Insert(TEntity entity);

        Task<long> InsertAsync(TEntity entity);

        int BulkInsert(IEnumerable<TEntity> entities);

        Task<int> BulkInsertAsync(IEnumerable<TEntity> entities);

        int Count(object condition);

        Task<int> CountAsync(object condition);
    }
}