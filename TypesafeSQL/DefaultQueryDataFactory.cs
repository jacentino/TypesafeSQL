using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The default implementation of query data factory.
    /// </summary>
    public class DefaultQueryDataFactory : IQueryDataFactory
    {
        private INameResolver nameResolver;

        /// <summary>
        /// Creates a new instance of a <see cref="c:DefaultQueryDataFactory"/> class.
        /// </summary>
        /// <param name="nameResolver">
        /// The name resolver.
        /// </param>
        public DefaultQueryDataFactory(INameResolver nameResolver)
        {
            this.nameResolver = nameResolver;
        }

        /// <summary>
        /// Creates the select query specification data object.
        /// </summary>
        /// <param name="fromData">
        /// The data passed to a from clause.
        /// </param>
        /// <param name="modelType">
        /// The type of a query model.
        /// </param>
        /// <returns>
        /// The <see cref="c:SelectQueryData"/> instance.
        /// </returns>
        public SelectQueryData CreateSelectQueryData(IQuerySource fromData, Type modelType = null)
        {
            return new SelectQueryData(new SqlCommandBuilder(nameResolver), fromData, modelType);
        }

        /// <summary>
        /// Creates the set operation (union/intersect/except) query specification data object.
        /// </summary>
        /// <param name="firstOperand">
        /// The first operand of an operation.
        /// </param>
        /// <param name="secondOperand">
        /// The second operand of an operation.
        /// </param>
        /// <param name="setOperator">
        /// The operator - UNION, INTERSECT or EXCEPT.
        /// </param>
        /// <returns>
        /// The <see cref="c:SetOperatorQueryData"/> instance.
        /// </returns>
        public SetOperatorQueryData CreateSetOperationQueryData(IQuery firstOperand, IQuery secondOperand, string setOperator)
        {
            return new SetOperatorQueryData(firstOperand, secondOperand, setOperator);
        }
    }
}
