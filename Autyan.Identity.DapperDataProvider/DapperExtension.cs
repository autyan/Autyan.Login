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

namespace Autyan.Identity.DapperDataProvider
{
    /// <summary>Dapper extensions.
    /// </summary>
    public static class DapperExtension
    {
        //The default concurrency multiplier is 4
        private static readonly int DefaultLevel = 4 * Environment.ProcessorCount;

        /// <summary>
        /// 对象缓存
        /// </summary>
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> ParamCache =
            new ConcurrentDictionary<Type, List<PropertyInfo>>(DefaultLevel, 100);

        /// <summary>
        /// 查询对象缓存
        /// </summary>
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> QueryParamCache =
            new ConcurrentDictionary<Type, List<PropertyInfo>>(DefaultLevel, 100);

        private static readonly Type[] DatabaseTypes = {
            typeof (int), typeof (long), typeof (byte), typeof (bool), typeof (short), typeof (string),typeof(decimal),
            typeof (int?), typeof (long?), typeof (byte?), typeof (bool?), typeof (short?),typeof(decimal?),
            typeof (DateTime),
            typeof (DateTime?)
        };

        public const string WhereIdEqualsId = " WHERE Id=@Id ";

        public const string NextValue = "NEXT VALUE FOR Ids";

        internal static string GetTableName(this Type type)
        {
            var metadata = GetMetadata(type);
            return metadata.TableName;
        }

        internal static string GetColumns(this Type type)
        {
            var metadata = GetMetadata(type);
            return string.Join(",", metadata.Columns);
        }


        /// <summary>Insert data into table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static long Insert(this IDbConnection connection, dynamic data, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = data as object;
            var properties = GetProperties(obj);
            var columns = string.Join(",", properties);
            var values = string.Join(",", properties.Select(p => "@" + p));
            var sql = $"INSERT INTO [{table}] ({columns}) VALUES ({values})";
            return connection.ExecuteScalar<long>(sql, obj, transaction, commandTimeout);
        }
        /// <summary>
        /// Insert data into table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static long Insert(this IDbConnection connection, object data, IDbTransaction transaction = null,
            int? commandTimeout = null)
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
            return Insert(connection, data, metadata.TableName, transaction, commandTimeout);
        }

        public static async Task<long> InsertAsync(this IDbConnection connection, object data,
            IDbTransaction transaction = null,
            int? commandTimeout = null)
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
            var obj = (object)data;
            var properties = GetProperties(obj);
            var columns = string.Join(",", properties);
            var values = string.Join(",", properties.Select(p => "@" + p));
            var sql = $"INSERT INTO [{metadata.TableName}] ({columns}) VALUES ({values})";
            return await connection.ExecuteScalarAsync<long>(sql, obj, transaction, commandTimeout);
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connection"></param>
        /// <param name="records">实体列表</param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>影响行数</returns>
        public static int BulkInsert<T>(this IDbConnection connection, T[] records, IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }
            var type = typeof(T);
            var builder = new StringBuilder();
            builder.AppendFormat("INSERT INTO [{0}]({1}) VALUES ",
                type.GetTableName(),
                type.GetColumns());
            var parameters = new DynamicParameters();
            var meta = GetMetadata(type);
            var useIdentity = meta.Key.Option == DatabaseGeneratedOption.Identity;
            var list = new List<string>();

            for (var index = 0; index < records.Length; index++)
            {
                var record = records[index];
                var paramList = new List<string>();
                if (useIdentity)
                {
                    paramList.Add(NextValue);
                }
                else
                {
                    paramList.Add("@Id_" + index);
                }
                paramList.AddRange(
                    meta.Properties.Where(o => o.Name != "Id").Select(o => $"@{o.Name}_{index}"));
                list.Add($"({string.Join(",", paramList)})");
                var expandoObject = new ExpandoObject() as IDictionary<string, object>;
                foreach (var property in meta.Properties)
                {
                    var value = property.PropertyInfo.GetValue(record);
                    expandoObject.Add(property.Name + "_" + index, value);
                }
                parameters.AddDynamicParams(expandoObject);
            }
            builder.Append(string.Join(",", list));
            return connection.Execute(builder.ToString(), parameters, transaction, commandTimeout);
        }
        public static async Task<int> BulkInsertAsync<T>(this IDbConnection connection, T[] records, IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }
            var type = typeof(T);
            var builder = new StringBuilder();
            builder.AppendFormat("INSERT INTO [{0}]({1}) VALUES ",
                type.GetTableName(),
                type.GetColumns());
            var parameters = new DynamicParameters();
            var meta = GetMetadata(type);
            var useIdentity = meta.Key.Option == DatabaseGeneratedOption.Identity;
            var list = new List<string>();

            for (var index = 0; index < records.Length; index++)
            {
                var record = records[index];
                var paramList = new List<string>();
                if (useIdentity)
                {
                    paramList.Add(NextValue);
                }
                else
                {
                    paramList.Add("@Id_" + index);
                }
                paramList.AddRange(
                    meta.Properties.Where(o => o.Name != "Id").Select(o => $"@{o.Name}_{index}"));
                list.Add($"({string.Join(",", paramList)})");
                var expandoObject = new ExpandoObject() as IDictionary<string, object>;
                foreach (var property in meta.Properties)
                {
                    var value = property.PropertyInfo.GetValue(record);
                    expandoObject.Add(property.Name + "_" + index, value);
                }
                parameters.AddDynamicParams(expandoObject);
            }
            builder.Append(string.Join(",", list));
            return await connection.ExecuteAsync(builder.ToString(), parameters, transaction, commandTimeout);
        }

        public static int BulkInsert<T>(this IDbConnection connection, IEnumerable<T> records, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var metadata = GetMetadata(typeof(T));
            var tableName = metadata.TableName;
            var column = metadata.Columns.ToList();
            var columns = string.Join(",", column);
            var values = string.Join(",", column.Select(p => "@" + p));
            var sql = $"insert into {tableName} ({columns}) Values ({values})";
            return connection.Execute(sql, records, transaction, commandTimeout);
        }

        /// <summary>Updata data for table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update(this IDbConnection connection, dynamic data, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var obj = data as object;
            var conditionObj = condition as object;

            var wherePropertyInfos = GetPropertyInfos(conditionObj, true);

            var whereFields = string.Empty;
            var objectValues = GetObjectValues(obj);
            var whereValues = GetObjectValues(conditionObj, true, true);
            if (whereValues.Count > 0)
            {
                whereFields = " WHERE " +
                              string.Join(" AND ",
                                  whereValues.Select(p => {
                                      var prop = wherePropertyInfos.Find(o => o.Name == p.Key);
                                      return string.Format(prop?.PropertyType.IsArray == true ? "{0} IN @w_{0}" : "{0}=@w_{0}", p.Key);
                                  }));
            }

            var updateFields = string.Join(",", objectValues.Keys.Select(k => k + "=@" + k));
            var sql = $"UPDATE [{table}] SET {updateFields}{whereFields}";
            var parameters = new DynamicParameters();
            parameters.AddDynamicParams(objectValues);
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;
            foreach (var whereValue in whereValues)
            {
                expandoObject.Add("w_" + whereValue.Key, whereValue.Value);
            }
            parameters.AddDynamicParams(expandoObject);

            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        public static int UpdateSelective<TEntity>(this IDbConnection connection, object data, dynamic condition = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var metadata = GetMetadata(typeof(TEntity));
            return UpdateSelective(connection, data, metadata.TableName, condition, transaction, commandTimeout);
        }

        public static async Task<int> UpdateSelectiveAsync<TEntity>(this IDbConnection connection, object data, dynamic condition = null,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var metadata = GetMetadata(typeof(TEntity));
            return await UpdateSelectiveAsync(connection, data, metadata.TableName, condition, transaction, commandTimeout);
        }

        /// <summary>
        /// 部分更新，不更新为null的字段
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <param name="tableName"></param>
        /// <param name="condition"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int UpdateSelective(this IDbConnection connection, object data, string tableName, dynamic condition = null, IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            var obj = data;
            var conditionObj = condition as object;

            var wherePropertyInfos = GetPropertyInfos(conditionObj, true);

            var objectValues = GetObjectValues(obj, true);
            var updateFields = string.Join(",", objectValues.Select(p => p.Key + " = @" + p.Key));
            var whereFields = string.Empty;

            var whereValues = GetObjectValues(conditionObj, true, true);
            if (whereValues.Any())
            {
                whereFields = " WHERE " +
                              string.Join(" AND ",
                                  whereValues.Select(p => {
                                      var prop = wherePropertyInfos.Find(o => o.Name == p.Key);
                                      return string.Format(prop?.PropertyType.IsArray == true ? "{0} IN @w_{0}" : "{0}=@w_{0}", p.Key);
                                  }));
            }
            var lastModifiedStr = string.Empty;
            if (!updateFields.Contains("LastModifiedAt"))
            {
                lastModifiedStr = ",LastModifiedAt=GETDATE()";
            }
            var sql = $"UPDATE [{tableName}] SET {updateFields}{lastModifiedStr} {whereFields}";

            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(objectValues);
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;
            foreach (var whereValue in whereValues)
            {
                expandoObject.Add("w_" + whereValue.Key, whereValue.Value);
            }
            parameters.AddDynamicParams(expandoObject);

            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        public static async Task<int> UpdateSelectiveAsync(this IDbConnection connection, object data, string tableName, dynamic condition = null, IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            var obj = data;
            var conditionObj = condition as object;

            var wherePropertyInfos = GetPropertyInfos(conditionObj, true);

            var objectValues = GetObjectValues(obj, true);
            var updateFields = string.Join(",", objectValues.Select(p => p.Key + " = @" + p.Key));
            var whereFields = string.Empty;

            var whereValues = GetObjectValues(conditionObj, true, true);
            if (whereValues.Count > 0)
            {
                whereFields = " WHERE " +
                              string.Join(" AND ",
                                  whereValues.Select(p => {
                                      var prop = wherePropertyInfos.Find(o => o.Name == p.Key);
                                      return string.Format(prop?.PropertyType.IsArray == true ? "{0} IN @w_{0}" : "{0}=@w_{0}", p.Key);
                                  }));
            }

            var sql = $"UPDATE [{tableName}] SET {updateFields}{whereFields}";

            var parameters = new DynamicParameters();

            parameters.AddDynamicParams(objectValues);
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;
            foreach (var whereValue in whereValues)
            {
                expandoObject.Add("w_" + whereValue.Key, whereValue.Value);
            }
            parameters.AddDynamicParams(expandoObject);

            return await connection.ExecuteAsync(sql, parameters, transaction, commandTimeout);
        }

        public static int UpdateByIdSelective<TEntity>(this IDbConnection connection, TEntity entity,
            IDbTransaction transaction = null, int? commandTimeout = null) where TEntity : BaseEntity
        {
            if (entity?.Id == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            var tableName = GetTableName(entity.GetType());

            var updatePropertyInfos = GetPropertyInfos(entity);

            var updateProperties = updatePropertyInfos.Where(p => p.Name != "Id" && p.GetValue(entity) != null).Select(p => p.Name);

            var updateFields = string.Join(",", updateProperties.Where(p => p != "Id").Select(p => p + " = @" + p));

            var sql = $"UPDATE [{tableName}] SET {updateFields}{WhereIdEqualsId}";

            return connection.ExecuteScalar<int>(sql, entity, transaction, commandTimeout);
        }

        public static async Task<int> UpdateByIdSelectiveAsync<TEntity>(this IDbConnection connection, TEntity entity,
            IDbTransaction transaction = null, int? commandTimeout = null) where TEntity : BaseEntity
        {
            if (entity?.Id == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            var tableName = GetTableName(entity.GetType());

            var updatePropertyInfos = GetPropertyInfos(entity);

            var updateProperties = updatePropertyInfos.Where(p => p.Name != "Id" && p.GetValue(entity) != null).Select(p => p.Name);

            var updateFields = string.Join(",", updateProperties.Where(p => p != "Id").Select(p => p + " = @" + p));

            var sql = $"UPDATE [{tableName}] SET {updateFields}{WhereIdEqualsId}";

            return await connection.ExecuteScalarAsync<int>(sql, entity, transaction, commandTimeout);
        }

        public static int Delete<TEntity>(this IDbConnection connection, dynamic condition,
            IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var metadata = GetMetadata(typeof(TEntity));
            var tableName = metadata.TableName;
            return Delete(connection, condition, tableName, transaction, commandTimeout);
        }


        public static async Task<int> DeleteAsync<TEntity>(this IDbConnection connection, dynamic condition,
        IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var metadata = GetMetadata(typeof(TEntity));
            var tableName = metadata.TableName;
            return await DeleteAsync(connection, condition, tableName, transaction, commandTimeout);
        }

        /// <summary>Delete data from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Delete(this IDbConnection connection, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var conditionObj = condition as object;
            var whereFields = string.Empty;
            var whereProperties = GetPropertyInfos(conditionObj, true);
            if (whereProperties.Count > 0)
            {
                whereFields = " WHERE " + string.Join(" AND ", whereProperties.Select(
                    p => string.Format(p.PropertyType.IsArray ? "{0} IN @{0}" : "{0}=@{0}", p.Name)));
            }

            var sql = $"DELETE FROM [{table}]{whereFields}";

            return connection.Execute(sql, conditionObj, transaction, commandTimeout);
        }

        public static async Task<int> DeleteAsync(this IDbConnection connection, dynamic condition, string table, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var conditionObj = condition as object;
            var whereFields = string.Empty;
            var whereProperties = GetPropertyInfos(conditionObj, true);
            if (whereProperties.Any())
            {
                whereFields = " WHERE " + string.Join(" AND ", whereProperties.Select(
                    p => string.Format(p.PropertyType.IsArray ? "{0} IN @{0}" : "{0}=@{0}", p.Name)));
            }

            var sql = $"DELETE FROM [{table}]{whereFields}";

            return await connection.ExecuteAsync(sql, conditionObj, transaction, commandTimeout);
        }

        public static int DeleteById<TEntity>(this IDbConnection connection, long id, IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            if (id <= 0)
            {
                throw new ArgumentNullException(nameof(id));
            }
            var tableName = GetTableName(typeof(TEntity));
            var sql = $"DELETE FROM [{tableName}]{WhereIdEqualsId}";
            return connection.Execute(sql, new { Id = id }, transaction, commandTimeout);
        }

        public static async Task<int> DeleteByIdAsync<TEntity>(this IDbConnection connection, long id, IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            if (id <= 0)
            {
                throw new ArgumentNullException(nameof(id));
            }
            var tableName = GetTableName(typeof(TEntity));
            var sql = $"DELETE FROM [{tableName}]{WhereIdEqualsId}";
            return await connection.ExecuteAsync(sql, new { Id = id }, transaction, commandTimeout);
        }
        /// <summary>Get data count from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int GetCount(this IDbConnection connection, object condition, string table, bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryList<int>(connection, condition, table, "COUNT(1)", isOr, transaction, commandTimeout).Single();
        }

        public static int GetCount<T>(this IDbConnection connection, object condition, IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            var tableName = GetTableName(typeof(T));
            return
                QueryList<int>(connection, condition, tableName, "COUNT(1)", false, transaction, commandTimeout)
                    .Single();
        }

        public static async Task<int> GetCountAsync<T>(this IDbConnection connection, object condition, IDbTransaction transaction = null,
            int? commandTimeout = null)
        {
            var type = typeof(T);
            var tableName = GetTableName(type);
            var result = await
                connection.QueryAsync<int>(BuildQuerySql(condition, tableName, "COUNT(1)"), condition,
                    transaction, commandTimeout);
            return result.Single();
        }

        public static TEntity GetById<TEntity>(this IDbConnection connection, long id)
        {
            return QueryList<TEntity>(connection, new { Id = id }).FirstOrDefault();
        }
        /// <summary>Query a list of data from table with a specified condition.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> QueryList(this IDbConnection connection, dynamic condition, string table, string columns = "*", bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return QueryList<dynamic>(connection, condition, table, columns, isOr, transaction, commandTimeout);
        }

        public static IEnumerable<T> QueryList<T>(this IDbConnection connection, object condition)
        {
            var type = typeof(T);
            return QueryList<T>(connection, condition, GetTableName(type), GetColumns(type));
        }

        public static Task<IEnumerable<T>> QueryListAsync<T>(this IDbConnection connection, object condition)
        {
            var type = typeof(T);
            return connection.QueryAsync<T>(BuildQuerySql(condition, GetTableName(type), GetColumns(type)),
                condition, null, null);
        }

        /// <summary>Query a list of data from table with specified condition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="isOr"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryList<T>(this IDbConnection connection, object condition, string table, string columns = "*", bool isOr = false, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.Query<T>(BuildQuerySql(condition, table, columns, isOr), condition, transaction, true, commandTimeout);
        }

        public static long Ids(this IDbConnection connection)
        {
            const string sql = "SELECT next value FOR Ids FROM sys.sequences";
            return connection.ExecuteScalar<long>(sql);
        }

        public static async Task<long> IdsAsync(this IDbConnection connection)
        {
            const string sql = "SELECT next value FOR Ids FROM sys.sequences";
            return await connection.ExecuteScalarAsync<long>(sql);
        }

        private static string BuildQuerySql(dynamic condition, string table, string selectPart = "*", bool isOr = false)
        {
            var conditionObj = condition as object;
            var properties = GetPropertyInfos(conditionObj, true);
            if (properties.Count == 0)
            {
                return $"SELECT {selectPart} FROM [{table}]";
            }

            var separator = isOr ? " OR " : " AND ";
            var wherePart = string.Join(separator,
                                  properties.Select(
                                      p => (IsArray(p.PropertyType)) ? p.Name + " IN @" + p.Name : p.Name + " = @" + p.Name));

            return $"SELECT {selectPart} FROM [{table}] WHERE {wherePart}";
        }

        private static List<string> GetProperties(object obj)
        {
            if (obj == null)
            {
                return new List<string>();
            }

            if (obj is DynamicParameters parameters)
            {
                return parameters.ParameterNames.ToList();
            }
            return GetPropertyInfos(obj).Select(x => x.Name).ToList();
        }

        /// <summary>
        /// 获取对象包含的属性列表
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="isParam">是否为查询对象</param>
        /// <returns></returns>
        private static List<PropertyInfo> GetPropertyInfos(object obj, bool isParam = false)
        {
            if (obj == null)
            {
                return new List<PropertyInfo>();
            }
            var type = obj.GetType();
            var meta = GetMetadata(type);
            if (meta == null)
            {
                List<PropertyInfo> properties;

                if (isParam)
                {
                    if (QueryParamCache.TryGetValue(type, out properties)) return properties.ToList();
                    properties =
                        obj.GetType()
                            .GetProperties().ToList();
                    properties = properties.Where(o => QueryPropertySupported(o.PropertyType)).ToList();
                    QueryParamCache[obj.GetType()] = properties;
                }
                else
                {
                    if (ParamCache.TryGetValue(type, out properties)) return properties.ToList();
                    properties =
                        obj.GetType()
                            .GetProperties().ToList();
                    properties = properties.Where(o => DatabaseTypes.Contains(o.PropertyType)).ToList();
                    ParamCache[obj.GetType()] = properties;
                }

                return properties;
            }
            return meta.Properties.Select(o => o.PropertyInfo).ToList();
        }

        private static DatabaseModelMetadata GetMetadata(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return MetadataContext.Instance[type];
        }

        /// <summary>
        /// 获取对象的值，以键值对形式返回，Key:属性名称，Value:属性值
        /// 如果对象为空，返回空字典
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="ignoreNullValues">是否忽略空值</param>
        /// <param name="isQueryParam">是否为查询对象</param>
        /// <returns></returns>
        private static IDictionary<string, object> GetObjectValues(object obj, bool ignoreNullValues = false, bool isQueryParam = false)
        {
            var dic = new Dictionary<string, object>();
            if (obj == null)
            {
                return dic;
            }
            foreach (var property in GetPropertyInfos(obj, isQueryParam))
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

        private static bool QueryPropertySupported(Type type)
        {
            return DatabaseTypes.Contains(type)
                   || (type.IsArray && DatabaseTypes.Contains(type.GetElementType()))
                   ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    && DatabaseTypes.Contains(type.GetGenericArguments()[0]));
        }

        private static bool IsArray(Type type)
        {
            return type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }
    }
}
