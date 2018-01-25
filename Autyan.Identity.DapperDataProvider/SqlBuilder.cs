using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autyan.Identity.Core.Data;

namespace Autyan.Identity.DapperDataProvider
{
    /// <summary>
    /// a simple sql builder
    /// </summary>
    public class SqlBuilder
    {
        private enum ActionType
        {
            /// <summary>
            /// insert
            /// </summary>
            Insert = 1,
            /// <summary>
            /// update
            /// </summary>
            Update,
            Delete,
            Select,
        }

        private SqlBuilder()
        {

        }
        /// <summary>
        /// sql builder begins
        /// </summary>
        /// <returns></returns>
        public static SqlBuilder Begin()
        {
            return new SqlBuilder();
        }

        /// <summary>
        /// action type
        /// </summary>
        private ActionType? _actionType;
        private readonly IList<string> _columns = new List<string>();
        private string _table;
        private readonly IList<KeyValuePair<string,string>> _values = new List<KeyValuePair<string, string>>();
        private readonly IList<string> _conditions = new List<string>();
        private int? _take;
        private int? _skip;
        private string _groupByFields;
        private const string Count = "COUNT(1)";

        private readonly IList<KeyValuePair<string, OrderDirection>> _orderByPairs =
            new List<KeyValuePair<string, OrderDirection>>();

        public SqlBuilder SelectCount()
        {
            _actionType = ActionType.Select;
            _columns.Add(Count);
            return this;
        }

        public SqlBuilder Select(string column)
        {
            _columns.Add(column);
            _actionType = ActionType.Select;
            return this;
        }

        public SqlBuilder From(string table)
        {
            _table = table;
            return this;
        }

        public SqlBuilder From(string tableFormat, params object[] args)
        {
            _table = string.Format(tableFormat, args);
            return this;
        }

        public SqlBuilder InsertInto(string table)
        {
            _table = table;
            _actionType = ActionType.Insert;
            return this;
        }

        public SqlBuilder Delete()
        {
            _actionType = ActionType.Delete;
            return this;
        }

        public SqlBuilder Update(string table)
        {
            _table = table;
            _actionType = ActionType.Update;
            return this;
        }

        public SqlBuilder Set(string key, string value)
        {
            _values.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public SqlBuilder Values(string key, string value)
        {
            _values.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public SqlBuilder Where()
        {
            return this;
        }

        public SqlBuilder And(string condition)
        {
            _conditions.Add(condition);
            return this;
        }

        public SqlBuilder GroupBy(string fields)
        {
            _groupByFields = fields;
            return this;
        }

        public SqlBuilder Take(int take)
        {
            _take = take;
            return this;
        }

        public SqlBuilder Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public SqlBuilder OrderBy(string field, OrderDirection? direction)
        {
            _orderByPairs.Add(new KeyValuePair<string, OrderDirection>(field, direction ?? OrderDirection.Desc));
            return this;
        }

        public string End()
        {
            var builder = new StringBuilder(100);
            if (_actionType == null)
            {
                throw new ArgumentNullException(nameof(_actionType));
            }

            switch (_actionType)
            {
                case ActionType.Update:
                    builder.Append("UPDATE ").Append(_table).Append(" SET ");
                    builder.Append(string.Join(", ", _values.Select(pair => pair.Key + "=" + pair.Value)));
                    break;
                case ActionType.Delete:
                    builder.Append("DELETE FROM ").Append(_table);
                    break;
                case ActionType.Insert:
                    builder.Append("INSERT INTO ").Append(_table);
                    builder.Append("(").Append(string.Join(",", _values.Select(p => p.Key))).Append(")");
                    builder.Append(" VALUES ");
                    builder.Append("(").Append(string.Join(",", _values.Select(p => p.Value))).Append(")");
                    break;
                case ActionType.Select:
                    builder.Append("SELECT ");
                    builder.Append(string.Join(",", _columns));
                    builder.Append(" FROM ").Append(_table);

                    break;
            }
            if (_conditions.Count > 0)
            {
                builder.Append(" WHERE ");
                builder.Append(string.Join(" AND ", _conditions));
            }
            if (!string.IsNullOrEmpty(_groupByFields))
            {
                builder.Append(" GROUP BY ").Append(_groupByFields);
            }
            if (ActionType.Select == _actionType)
            {
                if (_take == null)
                {
                    if (_orderByPairs?.Count > 0)
                    {
                        builder.Append(" ORDER BY ");
                        builder.Append(string.Join(",",
                            _orderByPairs.Select(o => o.Key + (o.Value == (OrderDirection.Asc) ? " ASC" : " DESC"))));
                        builder.Append(" ");
                    }
                }
                else
                {
                    if (_orderByPairs?.Count > 0)
                    {
                        builder.Append(" ORDER BY ");
                        builder.Append(string.Join(",",
                            _orderByPairs.Select(o => o.Key + (o.Value == (OrderDirection.Asc) ? " ASC" : " DESC"))));
                        builder.Append(" ");
                    }
                    builder.Append(" OFFSET ").Append(_skip ?? 0).Append(" ROWS FETCH NEXT ").Append(_take.Value).Append(" ROWS ONLY ");
                }
            }
            builder.Append(";");
            return builder.ToString();
        }

        public override string ToString()
        {
            return End();
        }
    }
}
