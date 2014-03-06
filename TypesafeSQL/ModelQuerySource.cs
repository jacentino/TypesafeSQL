using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The query source specifying a table using model type.
    /// </summary>
    public class ModelQuerySource : IQuerySource
    {
        private INameResolver nameResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="c:ModelQuerySource"/> class.
        /// </summary>
        /// <param name="modelType">
        /// The type of the model representing the table.
        /// </param>
        /// <param name="nameResolver">
        /// The name resolver customizing mapping between class name and table name.
        /// </param>
        public ModelQuerySource(Type modelType, INameResolver nameResolver)
        {
            ModelType = modelType;
            this.nameResolver = nameResolver;
        }

        /// <summary>
        /// Generates simple SELECT statement based on a model class name.
        /// </summary>
        /// <param name="subQueryPrefix">
        /// The prefix for alias and parameter names when the query is part of another query.
        /// </param>
        /// <returns>
        /// The SQL command as text and the dictionary od parameters.
        /// </returns>
        public ParameterizedSql GetSqlCommand(string subQueryPrefix)
        {
            return new ParameterizedSql
            {
                Command = "SELECT * FROM " + nameResolver.ResolveTableName(ModelType),
                Parameters = new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// Generates the table name.
        /// </summary>
        /// <param name="subQueryPrefix">
        /// Not used.
        /// </param>
        /// <returns>
        /// The table name as text and the empty dictionary of parameters.
        /// </returns>
        public ParameterizedSql GetSqlCommandOrTableName(string subQueryPrefix)
        {
            return new ParameterizedSql
            {
                Command = "[" + nameResolver.ResolveTableName(ModelType) + "]",
                Parameters = new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// Gets the model type.
        /// </summary>
        public Type ModelType
        {
            get;
            private set;
        }
    }
}
