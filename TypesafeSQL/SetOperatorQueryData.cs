using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The data collected by a set operator query instance.
    /// </summary>
    public class SetOperatorQueryData : IQuerySource
    {
        IQuery firstOperand; 
        IQuery secondOperand;
        string setOperator;

        /// <summary>
        /// Initializes a new instance of the <see cref="c:SetOperatorQueryData"/> class.
        /// </summary>
        /// <param name="commandBuilder">
        /// The object responsible for SELECT statement generation.
        /// </param>
        /// <param name="fromData">
        /// The data passed to the from clause.
        /// </param>
        public SetOperatorQueryData(IQuery firstOperand, IQuery secondOperand, string setOperator)
        {
            Check.NotNull(firstOperand, "firstOperand");
            Check.NotNull(secondOperand, "secondOperand");
            
            this.firstOperand = firstOperand;
            this.secondOperand = secondOperand;
            this.setOperator = setOperator;
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
            var firstSubQuerySql = firstOperand.Data.GetSqlCommand(subQueryPrefix + "so0_");
            var secondSubQuerySql = secondOperand.Data.GetSqlCommand(subQueryPrefix + "so1_");
            var parameters = firstSubQuerySql.Parameters;
            foreach (var param in secondSubQuerySql.Parameters)
                parameters[param.Key] = param.Value;
            return new ParameterizedSql
            {
                Command = string.Format("({0} {1} {2})", firstSubQuerySql.Command, setOperator, secondSubQuerySql.Command),
                Parameters = parameters
            };
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
            return GetSqlCommand(subQueryPrefix);
        }

        /// <summary>
        /// Gets the underlying model type.
        /// </summary>
        public Type ModelType
        {
            get { return firstOperand.Data.ModelType; }
        }
    }
}
