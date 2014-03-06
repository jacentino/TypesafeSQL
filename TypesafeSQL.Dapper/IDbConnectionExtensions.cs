using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Dynamic;

namespace TypesafeSQL.Dapper
{
    public static class IDbConnectionExtensions
    {
        public static IEnumerable<TModel> Query<TModel>(
            this IDbConnection connection, 
            IQuery<TModel> query, 
            IDbTransaction transaction = null, 
            bool buffered = true, 
            int? commandTimeout = null, 
            CommandType? commandType = null)
        {
            var sql = query.ToSql();
            IDictionary<string, object> expando = sql.Parameters; /*new ExpandoObject();
            foreach (var kv in sql.Parameters)
                expando[kv.Key] = kv.Value;*/
            return connection.Query<TModel>(sql.Command, expando, transaction, buffered, commandTimeout, commandType);
        }
    }
}
