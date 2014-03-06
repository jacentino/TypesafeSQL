using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The class representing a result of SQL generation.
    /// </summary>
    public class ParameterizedSql
    {
        /// <summary>
        /// The SQL command generated from a query.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Parameters of the SQL command.
        /// </summary>
        /// <remarks>
        /// All constant literals in linq specification of a query are translated to parameters too.
        /// </remarks>
        public IDictionary<string, object> Parameters { get; set; }
    }
}
