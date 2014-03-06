using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The data collected by a select query instance.
    /// </summary>
    public class SelectQueryData : IQuerySource
    {
        private SqlCommandBuilder commandBuilder;

        /// <summary>
        /// The class representing a join clause.
        /// </summary>
        public class JoinSpec
        {
            public IQuerySource InnerData { get; set; }
            public LambdaExpression InnerKeySelector { get; set; }
            public LambdaExpression OuterKeySelector { get; set; }
            public LambdaExpression ResultSelector { get; set; }
            public string JoinType { get; set; }
        }

        public IQuerySource FromData { get; set; }
        public Type ModelType { get { return FromData.ModelType; } }
        public bool Distinct { get; set; }
        public int TakeRows { get; set; }
        public int SkipRows { get; set; }
        public LambdaExpression WhereClause { get; set; }
        public LambdaExpression HavingClause { get; set; }
        public ICollection<Tuple<LambdaExpression, bool>> OrderByProperties { get; private set; }
        public ICollection<JoinSpec> Joins { get; private set; }
        public LambdaExpression SelectClause { get; set; }
        public LambdaExpression GroupByKey { get; set; }
        public LambdaExpression GroupByElement { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="c:SelectQueryData"/> class.
        /// </summary>
        /// <param name="commandBuilder">
        /// The object responsible for SELECT statement generation.
        /// </param>
        /// <param name="fromData">
        /// The data passed to the from clause.
        /// </param>
        public SelectQueryData(SqlCommandBuilder commandBuilder, IQuerySource fromData)
        {
            Check.NotNull(commandBuilder, "commandBuilder");
            Check.NotNull(fromData, "fromData");
            Distinct = false;
            TakeRows = 0;
            SkipRows = 0;
            OrderByProperties = new List<Tuple<LambdaExpression, bool>>();
            Joins = new List<JoinSpec>();
            FromData = fromData;
            this.commandBuilder = commandBuilder;

        }

        /// <summary>
        /// Generates SQL command based on a source.
        /// </summary>
        /// <param name="subQueryPrefix">
        /// The prefix for alias and parameter names when the query is part of another query.
        /// </param>
        /// <returns>
        /// The SQL command as text and the dictionary od parameters.
        /// </returns>
        public ParameterizedSql GetSqlCommand(string subQueryPrefix)
        {
            return commandBuilder.GetSelectCommand(this, subQueryPrefix);
        }

        /// <summary>
        /// Generates SQL command if some additional clauses specified, otherwise delegates call to the source.
        /// </summary>
        /// <param name="subQueryPrefix">
        /// The prefix for alias and parameter names when the query is part of another query.
        /// </param>
        /// <returns>
        /// The SQL command or table name as text and the dictionary of parameters.
        /// </returns>
        public ParameterizedSql GetSqlCommandOrTableName(string subQueryPrefix)
        {
            if (WhereClause == null && WhereClause == null && HavingClause == null &&
                Joins.Count == 0 && OrderByProperties.Count == 0 && GroupByKey == null)
                return FromData.GetSqlCommandOrTableName(subQueryPrefix);
            else
            {
                var sql = commandBuilder.GetSelectCommand(this, subQueryPrefix);
                sql.Command = "(" + sql.Command + ")";
                return sql;
            }
        }
    }
}
