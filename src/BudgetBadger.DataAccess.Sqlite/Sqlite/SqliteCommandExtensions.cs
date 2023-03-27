using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace BudgetBadger.DataAccess.Sqlite
{
	public static class SqliteCommandExtensions
	{
        public static void AddParameter(this SqliteCommand sqlCommand, string name, object value, SqliteType sqliteType)
        {
            sqlCommand.Parameters.Add(new SqliteParameter(name, value ?? DBNull.Value)).SqliteType = sqliteType;
        }

        public static string AddParameters(this SqliteCommand sqlCommand, string name, IEnumerable<object> values, SqliteType sqliteType)
        {
            var parameters = new List<string>();
            int i = 0;
            foreach (var value in values)
            {
                var parameter = $"{name}{i}";
                parameters.Add(parameter);
                sqlCommand.Parameters.Add(new SqliteParameter(parameter, value ?? DBNull.Value)).SqliteType = sqliteType;
                i++;
            }

            return string.Join(", ", parameters);
        }
    }
}

