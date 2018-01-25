using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Autyan.Identity.Core.Data;
using Autyan.Identity.Core.DataProvider;

namespace Autyan.Identity.DapperDataProvider
{
    public class BaseDapperDataProvider<TEntity, TQuery> : IDataProvider<TEntity, TQuery>
        where TEntity : BaseEntity
        where TQuery : BaseQuery<TEntity>
    {
        private IDbConnection _connection;

        protected IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = Factory.GetConnection(DbConnectionName.Default);
                }
                return _connection;
            }
        }

        protected readonly IDbConnectionFactory Factory;

        public BaseDapperDataProvider()
        {
            Factory = new DefaultDbConnectionFactory();
        }

        #region Interface Implemention
        int IDataProvider<TEntity, TQuery>.DeleteById(TEntity entity)
        {
            return DeleteById(entity);
        }

        async Task<int> IDataProvider<TEntity, TQuery>.DeleteByIdAsync(TEntity entity)
        {
            return await DeleteByIdAsync(entity);
        }

        int IDataProvider<TEntity, TQuery>.DeleteByCondition(object condition)
        {
            return DeleteByCondition(condition);
        }

        async Task<int> IDataProvider<TEntity, TQuery>.DeleteByConditionAsync(object condition)
        {
            return await DeleteByConditionAsync(condition);
        }

        PagedResult<TEntity> IDataProvider<TEntity, TQuery>.PagingQuery(TQuery query)
        {
            return PageingQuery(query);
        }

        async Task<PagedResult<TEntity>> IDataProvider<TEntity, TQuery>.PagingQueryAsync(TQuery query)
        {
            return await PagingQueryAsync(query);
        }

        long IDataProvider<TEntity, TQuery>.Insert(TEntity entity)
        {
            return Insert(entity);
        }

        async Task<long> IDataProvider<TEntity, TQuery>.InsertAsync(TEntity entity)
        {
            return await InsertAsync(entity);
        }

        int IDataProvider<TEntity, TQuery>.BulkInsert(IEnumerable<TEntity> entities)
        {
            return BulkInsert(entities);
        }

        async Task<int> IDataProvider<TEntity, TQuery>.BulkInsertAsync(IEnumerable<TEntity> entities)
        {
            return await BulkInsertAsync(entities);
        }

        int IDataProvider<TEntity, TQuery>.Count(object condition)
        {
            return Count(condition);
        }

        async Task<int> IDataProvider<TEntity, TQuery>.CountAsync(object condition)
        {
            return await CountAsync(condition);
        }

        int IDataProvider<TEntity, TQuery>.UpdateById(TEntity entity)
        {
            return UpdateById(entity);
        }

        async Task<int> IDataProvider<TEntity, TQuery>.UpdateByIdAsync(TEntity entity)
        {
            return await UpdateByIdAsync(entity);
        }

        int IDataProvider<TEntity, TQuery>.UpdateByCondition(object data, object condition)
        {
            return UpdateByCondition(data, condition);
        }

        async Task<int> IDataProvider<TEntity, TQuery>.UpdateByConditionAsync(object data, object condition)
        {
            return await UpdateByConditionAsync(data, condition);
        }

        IEnumerable<TEntity> IDataProvider<TEntity, TQuery>.Where(object query)
        {
            return Where(query);
        }

        async Task<IEnumerable<TEntity>> IDataProvider<TEntity, TQuery>.WhereAsync(object query)
        {
            return await WhereAsync(query);
        }

        public void Dispose()
        {
        }
        #endregion

        protected virtual long Insert(TEntity entity)
        {
            entity.Id = Connection.Ids();
            return Connection.Insert(entity);
        }

        protected virtual async Task<long> InsertAsync(TEntity entity)
        {
            entity.Id = await Connection.IdsAsync();
            return await Connection.InsertAsync(entity);
        }

        protected virtual IEnumerable<TEntity> Where(object query)
        {
            return Connection.QueryList<TEntity>(query);
        }

        protected virtual async Task<IEnumerable<TEntity>> WhereAsync(object query)
        {
            return await Connection.QueryListAsync<TEntity>(query);
        }

        protected virtual int DeleteById(TEntity entity)
        {
            if(entity.Id == null) throw new ArgumentNullException(nameof(entity.Id));
            return Connection.DeleteById<TEntity>(entity.Id.Value);
        }

        protected virtual async Task<int> DeleteByIdAsync(TEntity entity)
        {
            if (entity.Id == null) throw new ArgumentNullException(nameof(entity.Id));
            return await Connection.DeleteByIdAsync<TEntity>(entity.Id.Value);
        }

        protected virtual int DeleteByCondition(object condition)
        {
            return Connection.Delete<TEntity>(condition);
        }

        protected virtual async Task<int> DeleteByConditionAsync(object condition)
        {
            return await Connection.DeleteAsync<TEntity>(condition);
        }

        protected virtual int UpdateById(TEntity entity)
        {
            return Connection.UpdateByIdSelective(entity);
        }

        protected virtual async Task<int> UpdateByIdAsync(TEntity entity)
        {
            return await Connection.UpdateByIdSelectiveAsync(entity);
        }

        protected virtual int Count(object condition)
        {
            return Connection.GetCount<TEntity>(condition);
        }

        protected virtual async Task<int> CountAsync(object condition)
        {
            return await Connection.GetCountAsync<TEntity>(condition);
        }

        protected virtual int UpdateByCondition(object data, object condition)
        {
            return Connection.UpdateSelective<TEntity>(data, condition);
        }

        protected virtual async Task<int> UpdateByConditionAsync(object data, object condition)
        {
            return await Connection.UpdateSelectiveAsync<TEntity>(data, condition);
        }

        protected virtual int BulkInsert(IEnumerable<TEntity> entities)
        {
            return Connection.BulkInsert(entities.ToArray());
        }

        protected virtual async Task<int> BulkInsertAsync(IEnumerable<TEntity> entities)
        {
            return await Connection.BulkInsertAsync(entities.ToArray());
        }

        protected virtual PagedResult<TEntity> PageingQuery(TQuery query)
        {
            return null;
        }

        protected virtual async Task<PagedResult<TEntity>> PagingQueryAsync(TQuery query)
        {
            return await Task.FromResult(new PagedResult<TEntity>());
        }

        protected virtual void AppendQueryParams(TQuery query)
        {

        }
    }
}
