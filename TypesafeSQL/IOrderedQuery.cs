using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The interface for queries for which some order has been specified.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model class.
    /// </typeparam>
    public interface IOrderedQuery<TModel> : IQuery<TModel>
    {
        /// <summary>
        /// Adds another property to order specification.
        /// </summary>
        /// <param name="selector">
        /// The expression specifying ordering property.
        /// </param>
        /// <returns>
        /// The query specification with ordering criteria added.
        /// </returns>
        IOrderedQuery<TModel> ThenBy(Expression<Func<TModel, object>> selector);

        /// <summary>
        /// Adds another property to order specification with descending order.
        /// </summary>
        /// <param name="selector">
        /// The expression specifying ordering property.
        /// </param>
        /// <returns>
        /// The query specification with ordering criteria added.
        /// </returns>
        IOrderedQuery<TModel> ThenByDescending(Expression<Func<TModel, object>> selector);
    }
}
