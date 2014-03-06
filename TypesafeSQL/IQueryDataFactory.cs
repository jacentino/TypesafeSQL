using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL
{
    /// <summary>
    /// The interface for factories creating query specification data objects.
    /// </summary>
    public interface IQueryDataFactory
    {
        /// <summary>
        /// Creates the select query specification data object.
        /// </summary>
        /// <param name="fromData">
        /// The data passed to a from clause.
        /// </param>
        /// <returns>
        /// The <see cref="c:SelectQueryData"/> instance.
        /// </returns>
        SelectQueryData CreateSelectQueryData(IQuerySource fromData);

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
        SetOperatorQueryData CreateSetOperationQueryData(IQuery firstOperand, IQuery secondOperand, string setOperator);
    }
}
