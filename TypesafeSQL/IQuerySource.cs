using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The interface for data source specifications i.e. objects, that can be used as from clause arguments.
    /// </summary>
    public interface IQuerySource
    {
        /// <summary>
        /// Generates SQL command based on a source.
        /// </summary>
        /// <param name="subQueryPrefix">
        /// The prefix for alias and parameter names when the query is part of another query.
        /// </param>
        /// <returns>
        /// The SQL command as text and the dictionary od parameters.
        /// </returns>
        ParameterizedSql GetSqlCommand(string subQueryPrefix);

        /// <summary>
        /// Generates SQL command or table name if no additional clauses specified, based on a source.
        /// </summary>
        /// <param name="subQueryPrefix">
        /// The prefix for alias and parameter names when the query is part of another query.
        /// </param>
        /// <returns>
        /// The SQL command or table name as text and the dictionary of parameters.
        /// </returns>
        ParameterizedSql GetSqlCommandOrTableName(string subQueryPrefix);

        /// <summary>
        /// Gets the type of model class used in a source specification.
        /// </summary>
        Type ModelType { get; }
    }
}
