using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The class responsible for query instances creation.
    /// </summary>
    public class QueryBuilder
    {
        private INameResolver nameResolver;
        private IQueryDataFactory queryDataFactory;

        /// <summary>
        /// Initializes a new instance of the QueryBuilder class.
        /// </summary>
        /// <param name="nameResolver">
        /// The table/column name resolver.
        /// </param>
        public QueryBuilder(INameResolver nameResolver = null)
        {
            this.nameResolver = nameResolver ?? new DefaultNameResolver();
            this.queryDataFactory = new DefaultQueryDataFactory(this.nameResolver);
        }

        /// <summary>
        /// Creates a typed query for a given model class.
        /// </summary>
        /// <typeparam name="TModel">
        /// The model class.
        /// </typeparam>
        /// <returns>
        /// The query instance.
        /// </returns>
        public IQuery<TModel> Table<TModel>()
        {
            var fromData = new ModelQuerySource(typeof(TModel), nameResolver);
            return new SelectQuery<TModel>(queryDataFactory.CreateSelectQueryData(fromData), queryDataFactory);
        }
    }
}
