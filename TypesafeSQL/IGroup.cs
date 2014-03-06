using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The interface representing result of grouping model elements.
    /// </summary>
    /// <typeparam name="TKey">
    /// The grouping key type.
    /// </typeparam>
    /// <typeparam name="TModel">
    /// The model type.
    /// </typeparam>
    public interface IGroup<TKey, TModel>
    {
        /// <summary>
        /// Gets a grouping key.
        /// </summary>
        TKey Key { get; }

        /// <summary>
        /// Specifies sum aggregation function in a query clause.
        /// </summary>
        /// <typeparam name="TResult">
        /// Type of result of sum.
        /// </typeparam>
        /// <param name="selector">
        /// The expression representing model property on which the sum is calculated.
        /// </param>
        /// <returns>
        /// The sum of values of property of instances of the aggregable.
        /// </returns>
        TResult Sum<TResult>(Expression<Func<TModel, TResult>> selector);

        /// <summary>
        /// Specifies average aggregation function in a query clause.
        /// </summary>
        /// <typeparam name="TResult">
        /// Type of result of average.
        /// </typeparam>
        /// <param name="selector">
        /// The expression representing model property on which the average is calculated.
        /// </param>
        /// <returns>
        /// The average of values of property of instances of the aggregable.
        /// </returns>
        TResult Average<TResult>(Expression<Func<TModel, TResult>> selector);

        /// <summary>
        /// Specifies min aggregation function in a query clause.
        /// </summary>
        /// <typeparam name="TResult">
        /// Type of result of min.
        /// </typeparam>
        /// <param name="selector">
        /// The expression representing model property on which the min is calculated.
        /// </param>
        /// <returns>
        /// The min of values of property of instances of the aggregable.
        /// </returns>
        TResult Min<TResult>(Expression<Func<TModel, TResult>> selector);

        /// <summary>
        /// Specifies max aggregation function in a query clause.
        /// </summary>
        /// <typeparam name="TResult">
        /// Type of result of max.
        /// </typeparam>
        /// <param name="selector">
        /// The expression representing model property on which the max is calculated.
        /// </param>
        /// <returns>
        /// The max of values of property of instances of the aggregable.
        /// </returns>
        TResult Max<TResult>(Expression<Func<TModel, TResult>> selector);

        /// <summary>
        /// Specifies count aggregation function in a query clause.
        /// </summary>
        /// <returns>
        /// The count of elements of the aggregable.
        /// </returns>
        int Count();

        /// <summary>
        /// Specifies count aggregation function in a query clause.
        /// </summary>
        /// <param name="selector">
        /// The expression representing model property on which the count is calculated - only non-null values are counted.
        /// </param>
        /// <returns>
        /// The count of elements of the aggregable.
        /// </returns>
        int Count<TResult>(Expression<Func<TModel, TResult>> selector);

    }
}
