using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autyan.Identity.Core.Data;
using Autyan.Identity.Core.DataConfig;
using Autyan.Identity.Core.DataProvider;
using Dapper;

namespace Autyan.Identity.DapperDataProvider
{
    public class BaseDapperDataProvider<TEntity, TQuery> :  IDataProvider<TEntity, TQuery>
        where TEntity : BaseEntity
        where TQuery : BaseQuery<TEntity>
    {
        private IDbConnection _connection;

        protected IDbConnection Connection => _connection ?? (_connection = Factory.GetConnection(DbConnectionName.Default));

        protected readonly IDbConnectionFactory Factory;

        protected DatabaseModelMetadata Metadata { get; }

        protected string TableName => Metadata.TableName;

        protected IEnumerable<string> Columns => Metadata.Columns;

        public BaseDapperDataProvider()
        {
            Factory = new DefaultDbConnectionFactory();
            Metadata = MetadataContext.Instance[typeof(TEntity)];
        }

        public void Dispose()
        {
        }

        public virtual async Task<IEnumerable<TEntity>> QueryAsync(TQuery query)
        {
            var builder = BuildQuerySql(query);

            return await Connection.QueryAsync<TEntity>(builder.ToString(), query);
        }

        public virtual Task<int> DeleteByIdAsync(long? id)
        {
            var builder = new StringBuilder();
            builder.Append("DELETE FROM ").Append(TableName).Append(" WHERE Id = @Id");

            return Connection.ExecuteAsync(builder.ToString(), new {Id = id});
        }

        public virtual async Task<int> UpdateByIdAsync(TEntity entity)
        {
            entity.ModifiedAt = DateTime.Now;
            var builder = new StringBuilder();
            builder.Append("UPDATE ").Append(TableName).Append(" SET ");
            builder.Append(string.Join(",", Columns.Where(c => c != "Id").Select(c => $"{c} = @{c}")));
            builder.Append(" WHERE Id = @Id");
            entity.ModifiedAt = DateTime.Now;

            return await Connection.ExecuteAsync(builder.ToString(), entity);
        }

        public virtual async Task<PagedResult<TEntity>> PagingQueryAsync(TQuery query)
        {
            if (query.Take == null) throw new ArgumentNullException(nameof(query.Take));
            var queryBuilder = BuildQuerySql(query);
            queryBuilder.Append("OFFSET ").Append(query.Skip ?? 0).Append(" ROWS FETCH NEXT ").Append(query.Take)
                .Append(" ROWS ONLY");
            var results = await Connection.QueryAsync<TEntity>(queryBuilder.ToString(), query);

            var countBuilder = BuildCountSql(query);
            var count = await Connection.QueryAsync<int>(countBuilder.ToString(), query);

            return new PagedResult<TEntity>
            {
                Results = results,
                TotalCount = count.Single()
            };
        }

        public virtual async Task<long> InsertAsync(TEntity entity)
        {
            var builder = new StringBuilder();
            builder.Append("INSERT INTO ").Append(TableName).Append(" ( ")
                .Append(string.Join(", ", Columns)).Append(" ) VALUES (")
                .Append(string.Join(", ", Columns.Select(c => $"@{c}")))
                .Append(" )");
            entity.Id = await GetNextEntityIdAsync();
            entity.CreatedAt = DateTime.Now;
            return await Connection.ExecuteAsync(builder.ToString(), entity);
        }

        public virtual async Task<int> GetCountAsync(object condition)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT COUNT(1) FROM ").Append(TableName).Append(" WHERE 1= 1");
            var result = await Connection.QueryAsync<int>(builder.ToString(), condition);
            return result.Single();
        }

        protected virtual async Task<long> GetNextEntityIdAsync()
        {
            const string sql = "SELECT NEXT VALUE FOR EntityId FROM sys.sequences";
            return await Connection.ExecuteScalarAsync<long>(sql);
        }

        protected virtual StringBuilder BuildQuerySql(TQuery query)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT ").Append(string.Join(",", Columns)).Append(" FROM ").Append(TableName).Append(" WHERE 1= 1");
            AppendWhere(builder, query);

            return builder;
        }

        protected virtual StringBuilder BuildCountSql(TQuery query)
        {
            var builder = new StringBuilder();
            builder.Append("SELECT COUNT(1) FROM ").Append(TableName).Append(" WHERE 1= 1");
            AppendWhere(builder, query);

            return builder;
        }

        protected virtual void AppendWhere(StringBuilder builder, TQuery query)
        {
            if (query.Id != null)
            {
                builder.Append(" AND Id = @Id ");
            }

            if (query.IdFrom != null)
            {
                builder.Append(" AND Id > @IdFrom");
            }

            if (query.IdTo != null)
            {
                builder.Append(" AND Id < @IdTo");
            }

            if (query.Ids != null)
            {
                builder.Append(" AND Id IN @Ids");
            }

            if (query.CreatedAtFrom != null)
            {
                builder.Append(" AND CreatedAt > @CreatedAtFrom");
            }

            if (query.CreatedAtTo != null)
            {
                builder.Append(" AND CreatedAt < @CreatedAtTo");
            }

            if (query.LastModifiedAtFrom != null)
            {
                builder.Append(" AND ModifiedAt > @LastModifiedAtFrom");
            }

            if (query.LastModifiedAtTo != null)
            {
                builder.Append(" AND ModifiedAt < @LastModifiedAtTo");
            }
        }
    }
}
