using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The base, untyped interface of a query specification.
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Generates sql command with a dictionary of parameters from the query specification.
        /// </summary>
        /// <returns>
        /// The sql command with parameters.
        /// </returns>
        ParameterizedSql ToSql();

        /// <summary>
        /// Gets the query specification data - set of expressions representing clauses.
        /// </summary>
        IQuerySource Data { get; }
    }

    /// <summary>
    /// The class exposing DSL for query definition based on a given model class properties.
    /// </summary>
    /// <typeparam name="TModel">
    /// The model class.
    /// </typeparam>
    public interface IQuery<TModel> : IQuery
    {
        /// <summary>
        /// Adds filtering predicate to the query.
        /// </summary>
        /// <param name="predicate">
        /// The predicate filtering model instances.
        /// </param>
        /// <returns>
        /// The query specification with filtering criteria added.
        /// </returns>
        IQuery<TModel> Where(Expression<Predicate<TModel>> predicate);

        /// <summary>
        /// Specifies projection on a query data.
        /// </summary>
        /// <typeparam name="TResultModel">
        /// Class or value type representing result of projection.
        /// </typeparam>
        /// <param name="projection">
        /// Expression specifying transformation from a query model to a result model.
        /// </param>
        /// <returns>
        /// The query specification with projection added.
        /// </returns>
        IQuery<TResultModel> Select<TResultModel>(Expression<Func<TModel, TResultModel>> projection);

        /// <summary>
        /// Specifies ordering of query by a given model property.
        /// </summary>
        /// <param name="selector">
        /// The selector determining a ordering property.
        /// </param>
        /// <returns>
        /// The query specification with ordering specified.
        /// </returns>
        IOrderedQuery<TModel> OrderBy(Expression<Func<TModel, object>> selector);

        /// <summary>
        /// Specifies ordering of query by a given model property, descending.
        /// </summary>
        /// <param name="selector">
        /// The selector determining a ordering property.
        /// </param>
        /// <returns>
        /// The query specification with ordering specified.
        /// </returns>
        IOrderedQuery<TModel> OrderByDescending(Expression<Func<TModel, object>> selector);

        /// <summary>
        /// Specifies, that the query result can not contain duplicates.
        /// </summary>
        /// <returns>
        /// The query specification with distinct option specified.
        /// </returns>
        IQuery<TModel> Distinct();

        /// <summary>
        /// Specifies, that only <c>count</c> elements are to be retrieved from query result.
        /// </summary>
        /// <param name="count">
        /// The number of rows to take.
        /// </param>
        /// <returns>The query with LIMIT clause added.</returns>
        IQuery<TModel> Take(int count);

        /// <summary>
        /// Specifies, that first <c>count</c> elements of query must be ommitted.
        /// </summary>
        /// <param name="count">
        /// The number of rows to skip.
        /// </param>
        /// <returns>The query with OFFSET clause added.</returns>
        IQuery<TModel> Skip(int count);

        /// <summary>
        /// Joins query with another query by a specified key.
        /// </summary>
        /// <typeparam name="TInnerModel">
        /// The model of a query to join.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The join key type.
        /// </typeparam>
        /// <typeparam name="TResultModel">
        /// The model of join result.
        /// </typeparam>
        /// <param name="inner">
        /// The query to join with.
        /// </param>
        /// <param name="outerKeySelector">
        /// The expression specifying an outer key.
        /// </param>
        /// <param name="innerKeySelector">
        /// The expression specifying an inner key.
        /// </param>
        /// <param name="resultSelector">
        /// The expression specifying a projection of inner and outer models to result model.
        /// </param>
        /// <returns>
        /// The query specification of result of join.
        /// </returns>
        IQuery<TResultModel> Join<TInnerModel, TKey, TResultModel>(
            IQuery<TInnerModel> inner,
            Expression<Func<TModel, TKey>> outerKeySelector,
            Expression<Func<TInnerModel, TKey>> innerKeySelector,
            Expression<Func<TModel, TInnerModel, TResultModel>> resultSelector);

        /// <summary>
        /// Specifies left join of a query with another query by a specified key.
        /// </summary>
        /// <typeparam name="TInnerModel">
        /// The model of a query to join.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The join key type.
        /// </typeparam>
        /// <typeparam name="TResultModel">
        /// The model of join result.
        /// </typeparam>
        /// <param name="inner">
        /// The query to join with.
        /// </param>
        /// <param name="outerKeySelector">
        /// The expression specifying an outer key.
        /// </param>
        /// <param name="innerKeySelector">
        /// The expression specifying an inner key.
        /// </param>
        /// <param name="resultSelector">
        /// The expression specifying a projection of inner and outer models to result model.
        /// </param>
        /// <returns>
        /// The query specification of result of join.
        /// </returns>
        IQuery<TResultModel> LeftJoin<TInnerModel, TKey, TResultModel>(
            IQuery<TInnerModel> inner,
            Expression<Func<TModel, TKey>> outerKeySelector,
            Expression<Func<TInnerModel, TKey>> innerKeySelector,
            Expression<Func<TModel, TInnerModel, TResultModel>> resultSelector);

        /// <summary>
        /// Specifies right join of a query with another query by a specified key.
        /// </summary>
        /// <typeparam name="TInnerModel">
        /// The model of a query to join.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The join key type.
        /// </typeparam>
        /// <typeparam name="TResultModel">
        /// The model of join result.
        /// </typeparam>
        /// <param name="inner">
        /// The query to join with.
        /// </param>
        /// <param name="outerKeySelector">
        /// The expression specifying an outer key.
        /// </param>
        /// <param name="innerKeySelector">
        /// The expression specifying an inner key.
        /// </param>
        /// <param name="resultSelector">
        /// The expression specifying a projection of inner and outer models to result model.
        /// </param>
        /// <returns>
        /// The query specification of result of join.
        /// </returns>
        IQuery<TResultModel> RightJoin<TInnerModel, TKey, TResultModel>(
            IQuery<TInnerModel> inner,
            Expression<Func<TModel, TKey>> outerKeySelector,
            Expression<Func<TInnerModel, TKey>> innerKeySelector,
            Expression<Func<TModel, TInnerModel, TResultModel>> resultSelector);

        /// <summary>
        /// Specifies grouping of a query,
        /// </summary>
        /// <typeparam name="TKey">
        /// The grouping key type.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression specifying a grouping key.
        /// </param>
        /// <returns>
        /// The query specification with grouping specified.
        /// </returns>
        IQuery<IGroup<TKey, TModel>> GroupBy<TKey>(Expression<Func<TModel, TKey>> keySelector);

        /// <summary>
        /// Specifies grouping of a query,
        /// </summary>
        /// <typeparam name="TKey">
        /// The grouping key type.
        /// </typeparam>
        /// <typeparam name="TElement">
        /// The grouping element type.
        /// </typeparam>
        /// <param name="keySelector">
        /// The expression specifying a grouping key.
        /// </param>
        /// <param name="elementSelector">
        /// The expression specifying projection of a model to the grouping element.
        /// </param>
        /// <returns>
        /// The query specification with grouping specified.
        /// </returns>
        IQuery<IGroup<TKey, TElement>> GroupBy<TKey, TElement>(
            Expression<Func<TModel, TKey>> keySelector,
            Expression<Func<TModel, TElement>> elementSelector);

        /// <summary>
        /// Specifies union of operand queries.
        /// </summary>
        /// <param name="otherQuery">
        /// The query to concat.
        /// </param>
        /// <returns>
        /// Union of two queries.
        /// </returns>
        IQuery<TModel> Union(IQuery<TModel> otherQuery);

        /// <summary>
        /// Specifies intersection of operand queries.
        /// </summary>
        /// <param name="otherQuery">
        /// The query to intersect with.
        /// </param>
        /// <returns>
        /// The intersection of two queries.
        /// </returns>
        IQuery<TModel> Intersect(IQuery<TModel> otherQuery);

        /// <summary>
        /// Specifies difference between queries - set of elements belonging to the first collection
        /// and don't belonging to the second one.
        /// </summary>
        /// <param name="otherQuery">
        /// The query to calculate difference.
        /// </param>
        /// <returns>
        /// The difference between two queries.
        /// </returns>
        IQuery<TModel> Except(IQuery<TModel> otherQuery);

        /// <summary>
        /// Marker method allowing to specify that a particular value belongs to the query result if it is used as a subquery.
        /// </summary>
        /// <param name="element">
        /// The element to be searched for.
        /// </param>
        /// <returns>
        /// <c>true</c> if the element belongs to the result of the query, otherwise <c>false</c>.
        /// </returns>
        bool Contains(TModel element);
    }
}
