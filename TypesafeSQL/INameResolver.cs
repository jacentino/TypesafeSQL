using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The interface allowing for model-to-table name mapping customization.
    /// </summary>
    public interface INameResolver
    {
        /// <summary>
        /// Translates a model class name to a table name.
        /// </summary>
        /// <param name="modelClass">
        /// The model class.
        /// </param>
        /// <returns>
        /// The table name.
        /// </returns>
        string ResolveTableName(Type modelClass);

        /// <summary>
        /// Translates a model property/field name to a column name.
        /// </summary>
        /// <param name="property">
        /// The model property.
        /// </param>
        /// <returns>
        /// The column name.
        /// </returns>
        string ResolveColumnName(MemberInfo property);
    }
}
