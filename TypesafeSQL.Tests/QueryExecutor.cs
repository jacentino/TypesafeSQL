using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypesafeSQL;

namespace TypesafeSQL.Tests
{
    public class QueryExecutor
    {
        private string connectionString;

        public QueryExecutor(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IEnumerable<T> LoadObjects<T>(IQuery<T> query) where T : new()
        {
            var command = query.ToSql();
            return Execute<T>(command, reader => this.Instantiate<T>(reader));
        }

        public IEnumerable<T> LoadObjects<T>(string sql, IDictionary<string, object> parameters) where T : new()
        {
            var command = new ParameterizedSql
            {
                Command = sql,
                Parameters = parameters
            };
            return Execute<T>(command, reader => this.Instantiate<T>(reader));
        }

        public IEnumerable<T> LoadScalars<T>(IQuery<T> query)
        {
            var command = query.ToSql();
            return Execute<T>(command, reader =>
            {
                try
                {
                    return (T)reader.GetValue(0);
                }
                catch (InvalidCastException ex)
                {
                    return (T)Convert.ChangeType(reader.GetValue(0), typeof(T));
                }
            });
        }

        private IEnumerable<T> Execute<T>(ParameterizedSql command, Func<IDataReader, T> instantiate)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = command.Command;
                    foreach (var pair in command.Parameters)
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            yield return instantiate(reader);
                    }
                }
            }
        }

        private T Instantiate<T>(IDataReader reader) where T : new()
        {
            var instance = new T();
            foreach (var prop in instance.GetType().GetProperties())
            {
                object value = reader[prop.Name];
                prop.SetValue(instance, value == DBNull.Value ? null : value);
            }
            return instance;
        }
    }
}
