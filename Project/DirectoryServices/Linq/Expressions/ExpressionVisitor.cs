using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HansKindberg.Linq.Expressions;

namespace HansKindberg.DirectoryServices.Linq.Expressions
{
	public class ExpressionVisitor : BasicExpressionVisitor
	{
		#region Fields

		private static readonly IEnumerable<ExpressionType> _conditionalOperatorExpressionTypes = new HashSet<ExpressionType> {ExpressionType.And, ExpressionType.AndAlso, ExpressionType.Or, ExpressionType.OrElse};

		#endregion

		#region Properties

		protected internal virtual IEnumerable<ExpressionType> ConditionalOperatorExpressionTypes => _conditionalOperatorExpressionTypes;

		#endregion

		#region Methods

		protected internal virtual bool IsConditionalOperatorExpression(BinaryExpression expression)
		{
			return expression != null && this.ConditionalOperatorExpressionTypes.Contains(expression.NodeType);
		}

		protected override Expression VisitBinary(BinaryExpression expression)
		{
			if(this.IsConditionalOperatorExpression(expression))
			{
				// Do something.
			}
			else
			{
				//this._hqlExpression.Append("(");

				this.Visit(expression.Left);

				// In production code, handle this via lookup tables.
				switch(expression.NodeType)
				{
					case ExpressionType.Equal:
						//this._hqlExpression.Append(" = ");
						break;

					case ExpressionType.Add:
						//this._hqlExpression.Append(" + ");
						break;

					case ExpressionType.Subtract:
						//this._hqlExpression.Append(" - ");
						break;

					case ExpressionType.Multiply:
						//this._hqlExpression.Append(" * ");
						break;

					case ExpressionType.Divide:
						//this._hqlExpression.Append(" / ");
						break;

					default:
						base.VisitBinary(expression);
						break;
				}

				this.Visit(expression.Right);
				//this._hqlExpression.Append(")");
			}

			return expression;
		}

		#endregion
	}
}