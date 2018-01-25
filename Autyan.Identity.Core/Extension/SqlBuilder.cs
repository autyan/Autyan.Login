using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autyan.Identity.Core.Extension
{
    public class SqlBuilder
    {
        private Action? _action;

        private string _tableName;

        private string _selectIntoTable;

        private int? _skip;

        private int? _take;

        private readonly IList<string> _columns = new List<string>();

        private readonly IList<string> _whereClause = new List<string>();

        private readonly IList<string> _orderbyClause = new List<string>();

        private readonly IList<string> _groupbyClause = new List<string>();

        private readonly IList<string> _havingClause = new List<string>();

        private readonly IList<KeyValuePair<string, string>> _values = new List<KeyValuePair<string, string>>();

        private const string SelectCountColumn = " COUNT(1) ";

        private StringBuilder _strBuilder;

        private SqlBuilder()
        {

        }

        public static SqlBuilder Begin()
        {
            return new SqlBuilder();
        }

        private void CheckAction()
        {
            if (_action != null) throw new InvalidOperationException("action has set.");
        }

        public SqlBuilder InsertInto(string table)
        {
            CheckAction();
            _action = Action.Insert;
            _tableName = table;
            return this;
        }

        public SqlBuilder SelectCount()
        {
            CheckAction();
            _action = Action.Select;
            _columns.Add(SelectCountColumn);
            return this;
        }

        public SqlBuilder Select(string columns)
        {
            CheckAction();
            _action = Action.Select;
            _columns.Add(columns);
            return this;
        }

        public SqlBuilder SelectInto(string columns, string selectIntoTable)
        {
            _selectIntoTable = selectIntoTable;
            return Select(columns);
        }

        public SqlBuilder Update(string table)
        {
            CheckAction();
            _action = Action.Update;
            _tableName = table;
            return this;
        }

        public SqlBuilder Delete()
        {
            CheckAction();
            _action = Action.Delete;
            return this;
        }

        public SqlBuilder FromTable(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public SqlBuilder Set(string key, string value)
        {
            _values.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public SqlBuilder WhereAnd(string condition)
        {
            _whereClause.Add($" AND {condition}");
            return this;
        }

        public SqlBuilder WhereOr(string condition)
        {
            _whereClause.Add($" OR {condition}");
            return this;
        }

        public SqlBuilder GroupBy(string column)
        {
            _groupbyClause.Add(column);
            return this;
        }

        public SqlBuilder Having(string condition)
        {
            _havingClause.Add(condition);
            return this;
        }

        public SqlBuilder OrderBy(string column, bool isAsc = true)
        {
            _orderbyClause.Add(isAsc ? $" {column} " : $" {column} DESC");
            return this;
        }

        public SqlBuilder Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public SqlBuilder Take(int take)
        {
            _take = take;
            return this;
        }

        public string End()
        {
            if (_action == null) throw new ArgumentNullException(nameof(_action));
            _strBuilder = new StringBuilder();
            BuildAction();
            _strBuilder.Append(";");

            return _strBuilder.ToString();
        }

        private void BuildAction()
        {
            switch (_action)
            {
                case Action.Insert:
                    BuildInsert();
                    break;
                case Action.Select:
                    BuildSelect();
                    break;
                case Action.Update:
                    BuildUpdate();
                    break;
                case Action.Delete:
                    BuildDelete();
                    break;
            }
        }

        private void BuildInsert()
        {
            _strBuilder.Append("INSERT INTO ").Append(_tableName)
                .Append(" (").Append(string.Join(", ", _values.Select(v => v.Key)))
                .Append(") VALUES ")
                .Append(string.Join(", ", _values.Select(v => v.Value)));
        }

        private void BuildSelect()
        {
            _strBuilder.Append("SELECT ").Append(string.Join(", ", _columns));
            if (!string.IsNullOrWhiteSpace(_selectIntoTable))
            {
                _strBuilder.Append(" INTO ").Append(_selectIntoTable);
            }

            _strBuilder.Append(" FROM ").Append(_tableName);
            BuildWhere();

            //build group by clause
            if (_groupbyClause.Count > 0)
            {
                _strBuilder.Append(" GROUP BY ");
                _strBuilder.Append(string.Join(", ", _groupbyClause));
            }

            //build having clause
            if (_havingClause.Count > 0)
            {
                _strBuilder.Append(" HAVING ");
                _strBuilder.Append(string.Join(" AND ", _havingClause));
            }

            //build order by clause
            if (_orderbyClause.Count > 0)
            {
                _strBuilder.Append(" ORDER BY ");
                _strBuilder.Append(string.Join(", ", _orderbyClause));
            }

            //build offset fetch next
            if (_take != null)
            {
                _strBuilder.Append(" OFFSET ").Append(_skip ?? 0).Append(" ROWS FETCH NEXT ").Append(_take).Append(" ROWS ONLY");
            }
        }

        private void BuildUpdate()
        {
            _strBuilder.Append("UPDATE ").Append(_tableName).Append(" SET ")
                .Append(string.Join(", ", _values.Select(v => $"{v.Key} = {v.Value}")));
            BuildWhere();
        }

        private void BuildDelete()
        {
            _strBuilder.Append("DELETE FROM ").Append(_tableName);
            BuildWhere();
        }

        private void BuildWhere()
        {
            if (_whereClause.Count == 0) return;

            _strBuilder.Append(" WHERE (1=1) ");
            foreach (var condition in _whereClause)
            {
                _strBuilder.Append(condition);
            }
        }

        public override string ToString()
        {
            return End();
        }
    }

    public enum Action
    {
        Insert,

        Select,

        Update,

        Delete
    }
}
