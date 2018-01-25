using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autyan.Identity.Core.Data;
using Autyan.Identity.Core.DataConfig;
using Dapper;

namespace Autyan.Identity.Core.Extension
{
    public static class DapperExtension
    {
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> QueryParamtersCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();

        public static long Insert(this IDbConnection connection, object data, IDbTransaction transaction = null, 
            DatabaseGeneratedOption? option = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            var metadata = GetMetadata(data.GetType());
            return Insert(connection, data, metadata.TableName, option, transaction, commandTimeout, commandType);
        }

        public static async Task<long> InsertAsync(this IDbConnection connection, object data, IDbTransaction transaction = null,
            DatabaseGeneratedOption? option = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            var metadata = GetMetadata(data.GetType());
            return await InsertAsync(connection, data, metadata.TableName, option, transaction, commandTimeout, commandType);
        }

        public static long Insert(this IDbConnection connection, object data, string table, DatabaseGeneratedOption? option, 
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }
            var obj = data;
            var properties = GetProperties(obj);
            if (option == DatabaseGeneratedOption.Identity || option == DatabaseGeneratedOption.Computed)
            {
                properties = properties.Where(p => p.Name != "Id").ToList();
            }
            var columns = string.Join(",", properties.Select(p => p.Name));
            var values = string.Join(",", properties.Select(p => "@" + p.Name));
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("INSERT INTO ").Append(table).Append(" (").Append(columns).Append(") VALUES (")
                .Append(values).Append(");");
            return connection.ExecuteScalar<long>(sqlBuilder.ToString(), obj, transaction, commandTimeout, commandType);
        }

        public static async Task<long> InsertAsync(this IDbConnection connection, object data, string table, DatabaseGeneratedOption? option, 
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }
            var obj = data;
            var properties = GetProperties(obj);
            if (option == DatabaseGeneratedOption.Identity || option == DatabaseGeneratedOption.Computed)
            {
                properties = properties.Where(p => p.Name != "Id").ToList();
            }
            var columns = string.Join(",", properties.Select(p => p.Name));
            var values = string.Join(",", properties.Select(p => "@" + p.Name));
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("INSERT INTO ").Append(table).Append(" (").Append(columns).Append(") VALUES (")
                .Append(values).Append(");");
            return await connection.ExecuteScalarAsync<long>(sqlBuilder.ToString(), obj, transaction, commandTimeout, commandType);
        }

        public static TEntity GetById<TEntity>(this IDbConnection connection, long id)
        {
            return GetById<TEntity>(connection, new {Id = id});
        }

        public static async Task<TEntity> GetByIdAsnyc<TEntity>(this IDbConnection connection, long id)
        {
            return await GetByIdAsync<TEntity>(connection, new { Id = id });
        }

        public static TEntity GetById<TEntity>(this IDbConnection connection, object paramters,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var metadata = GetMetadata(typeof(TEntity));
            var columns = metadata.Columns;
            var table = metadata.TableName;
            var sql = $"SELECT ({columns} FROM [{table}] WHERE Id = @Id)";
            return connection.QueryFirstOrDefault<TEntity>(sql, paramters, transaction, commandTimeout, commandType);
        }

        public static async Task<TEntity> GetByIdAsync<TEntity>(this IDbConnection connection, object paramters,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var metadata = GetMetadata(typeof(TEntity));
            var columns = metadata.Columns;
            var table = metadata.TableName;
            var sql = $"SELECT ({columns} FROM [{table}] WHERE Id = @Id)";
            return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, paramters, transaction, commandTimeout, commandType);
        }

        public static int DeleteById<TEntity>(this IDbConnection connection, long id, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (id <= 0)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var metadata = GetMetadata(typeof(TEntity));
            var tableName = metadata.TableName;
            var sql = $"DELETE FROM [{tableName}] WHERE Id = @Id";
            return connection.Execute(sql, new {Id = id}, transaction, commandTimeout, commandType);
        }

        public static async Task<int> DeleteByIdAsync<TEntity>(this IDbConnection connection, long id, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType ? commandType = null)
        {
            if (id <= 0)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var metadata = GetMetadata(typeof(TEntity));
            var tableName = metadata.TableName;
            var sql = $"DELETE FROM [{tableName}] WHERE Id = @Id";
            return await connection.ExecuteAsync(sql, new { Id = id }, transaction, commandTimeout, commandType);
        }

        public static int UpdateById<TEntity>(this IDbConnection connection, TEntity entity,
            IDbTransaction transaction = null, int? commandTimeout = null) where TEntity : BaseEntity
        {
            if (entity?.Id == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var metadata = GetMetadata(typeof(TEntity));
            var tableName = metadata.TableName;
            var updateFields = string.Join(",", metadata.Columns.Where(c => c != "Id").Select(col => $"{col} = @{col}"));
            var sql = $"UPDATE [{tableName}] SET {updateFields} WHERE Id = @Id";

            return connection.Execute(sql, entity, transaction, commandTimeout);
        }

        public static async Task<int> UpdateByIdAsync<TEntity>(this IDbConnection connection, TEntity entity,
            IDbTransaction transaction = null, int? commandTimeout = null) where TEntity : BaseEntity
        {
            if (entity?.Id == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var metadata = GetMetadata(typeof(TEntity));
            var tableName = metadata.TableName;
            var updateFields = string.Join(",", metadata.Columns.Where(c => c != "Id").Select(col => $"{col} = @{col}"));
            var sql = $"UPDATE [{tableName}] SET {updateFields} WHERE Id = @Id";

            return await connection.ExecuteAsync(sql, entity, transaction, commandTimeout);
        }

        public static int UpdateByContdition<TEntity>(this IDbConnection connection, object paramters, dynamic condition,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (paramters == null)
            {
                throw new ArgumentNullException(nameof(paramters));
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            var metadata = GetMetadata(typeof(TEntity));
            var conditionObj = (object) condition;
            var updateColumns = GetProperties(paramters);
            var sqlbuilder = new StringBuilder();
            sqlbuilder.Append("UPDATE ").Append(metadata.TableName).Append(" SET ")
                .Append(string.Join(",", updateColumns.Select(col => $"w_{col.Name} = @w_{col.Name}")))
                .Append(" WHERE 1=1");
            foreach (var con in GetProperties(conditionObj))
            {
                sqlbuilder.Append(" AND ").Append(con.Name).Append(" = @").Append(con.Name);
            }

            sqlbuilder.Append(";");
            var parameters = new DynamicParameters();
            var objectValues = GetObjectValues(paramters);
            var whereValues = GetObjectValues((object)condition);
            parameters.AddDynamicParams(objectValues);
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;
            foreach (var whereValue in whereValues)
            {
                expandoObject.Add("w_" + whereValue.Key, whereValue.Value);
            }
            parameters.AddDynamicParams(expandoObject);

            return connection.Execute(sqlbuilder.ToString(), parameters, transaction, commandTimeout, commandType);
        }

        public static async Task<int> UpdateByContditionAsync<TEntity>(this IDbConnection connection, object paramters, dynamic condition,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (paramters == null)
            {
                throw new ArgumentNullException(nameof(paramters));
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            var metadata = GetMetadata(typeof(TEntity));
            var conditionObj = (object)condition;
            var updateColumns = GetProperties(paramters);
            var sqlbuilder = new StringBuilder();
            sqlbuilder.Append("UPDATE ").Append(metadata.TableName).Append(" SET ")
                .Append(string.Join(",", updateColumns.Select(col => $"w_{col.Name} = @w_{col.Name}")))
                .Append(" WHERE 1=1");
            foreach (var con in GetProperties(conditionObj))
            {
                sqlbuilder.Append(" AND ").Append(con.Name).Append(" = @").Append(con.Name);
            }

            sqlbuilder.Append(";");
            var parameters = new DynamicParameters();
            var objectValues = GetObjectValues(paramters);
            var whereValues = GetObjectValues((object)condition);
            parameters.AddDynamicParams(objectValues);
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;
            foreach (var whereValue in whereValues)
            {
                expandoObject.Add("w_" + whereValue.Key, whereValue.Value);
            }
            parameters.AddDynamicParams(expandoObject);

            return await connection.ExecuteAsync(sqlbuilder.ToString(), parameters, transaction, commandTimeout, commandType);
        }

        private static List<PropertyInfo> GetProperties(object obj)
        {
            if (obj == null)
            {
                return new List<PropertyInfo>();
            }

            //if (obj is DynamicParameters parameters)
            //{
            //    return parameters.ParameterNames.ToList();
            //}

            var type = obj.GetType();
            var metadata = MetadataContext.Instance[type];
            if (metadata != null)
            {
                return metadata.PropertyInfos.ToList();
            }

            if (QueryParamtersCache.ContainsKey(type))
            {
                return QueryParamtersCache[type].ToList();
            }

            var properties = type.GetProperties().ToList();
            QueryParamtersCache[type] = properties;
            return properties;
        }

        private static IDictionary<string, object> GetObjectValues(object obj, bool ignoreNullValues = false)
        {
            var dic = new Dictionary<string, object>();
            if (obj == null)
            {
                return dic;
            }

            foreach (var property in GetProperties(obj))
            {
                var value = property.GetValue(obj);
                if (ignoreNullValues && value == null)
                {
                    //ignore
                }
                else
                {
                    dic.Add(property.Name, value);
                }
            }
            return dic;
        }

        private static DatabaseModelMetadata GetMetadata(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return MetadataContext.Instance[type];
        }
    }
}
