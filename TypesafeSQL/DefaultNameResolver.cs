using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The default implementation of the name resolver.
    /// </summary>
    public class DefaultNameResolver : INameResolver
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
        public virtual string ResolveTableName(Type modelClass)
        {
            return modelClass.Name;
        }

        /// <summary>
        /// Translates a model property/field name to a column name.
        /// </summary>
        /// <param name="property">
        /// The model property.
        /// </param>
        /// <returns>
        /// The column name.
        /// </returns>
        public virtual string ResolveColumnName(System.Reflection.MemberInfo property)
        {
            return property.Name;
        }
    }
}
