//  This file is part of NHibernate.ReLinq.Sample a sample showing
//  the use of the open source re-linq library to implement a non-trivial 
//  Linq-provider, on the example of NHibernate (www.nhibernate.org).
//  Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
//  
//  NHibernate.ReLinq.Sample is based on re-motion re-linq (http://www.re-motion.org/).
//  
//  NHibernate.ReLinq.Sample is free software; you can redistribute it 
//  and/or modify it under the terms of the MIT License 
// (http://www.opensource.org/licenses/mit-license.php).
// 

using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;

namespace NHibernate.ReLinq.Sample.HqlQueryGeneration
{
	public class HqlGeneratorExpressionTreeVisitor : ThrowingExpressionTreeVisitor
	{
		#region Fields

		private readonly StringBuilder _hqlExpression = new StringBuilder();
		private readonly ParameterAggregator _parameterAggregator;

		#endregion

		#region Constructors

		private HqlGeneratorExpressionTreeVisitor(ParameterAggregator parameterAggregator)
		{
			this._parameterAggregator = parameterAggregator;
		}

		#endregion

		#region Methods

		// Called when a LINQ expression type is not handled above.
		protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
		{
			return new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The expression \"{0}\", of type \"{1}\", handled by visit-method \"{2}\" is not supported by this LINQ-provider.", unhandledItem, typeof(T), visitMethod));

			//string itemText = this.FormatUnhandledItem (unhandledItem);
			//var message = string.Format ("The expression '{0}' (type: {1}) is not supported by this LINQ provider.", itemText, typeof(T));
			//return new NotSupportedException (message);
		}

		private string FormatUnhandledItem<T>(T unhandledItem)
		{
			var itemAsExpression = unhandledItem as Expression;
			return itemAsExpression != null ? FormattingExpressionTreeVisitor.Format(itemAsExpression) : unhandledItem.ToString();
		}

		public static string GetHqlExpression(Expression linqExpression, ParameterAggregator parameterAggregator)
		{
			var visitor = new HqlGeneratorExpressionTreeVisitor(parameterAggregator);
			visitor.VisitExpression(linqExpression);
			return visitor.GetHqlExpression();
		}

		public string GetHqlExpression()
		{
			return this._hqlExpression.ToString();
		}

		protected override Expression VisitBinaryExpression(BinaryExpression expression)
		{
			this._hqlExpression.Append("(");

			this.VisitExpression(expression.Left);

			// In production code, handle this via lookup tables.
			switch(expression.NodeType)
			{
				case ExpressionType.Equal:
					this._hqlExpression.Append(" = ");
					break;

				case ExpressionType.AndAlso:
				case ExpressionType.And:
					this._hqlExpression.Append(" and ");
					break;

				case ExpressionType.OrElse:
				case ExpressionType.Or:
					this._hqlExpression.Append(" or ");
					break;

				case ExpressionType.Add:
					this._hqlExpression.Append(" + ");
					break;

				case ExpressionType.Subtract:
					this._hqlExpression.Append(" - ");
					break;

				case ExpressionType.Multiply:
					this._hqlExpression.Append(" * ");
					break;

				case ExpressionType.Divide:
					this._hqlExpression.Append(" / ");
					break;

				default:
					base.VisitBinaryExpression(expression);
					break;
			}

			this.VisitExpression(expression.Right);
			this._hqlExpression.Append(")");

			return expression;
		}

		protected override Expression VisitConstantExpression(ConstantExpression expression)
		{
			var namedParameter = this._parameterAggregator.AddParameter(expression.Value);
			this._hqlExpression.AppendFormat(":{0}", namedParameter.Name);

			return expression;
		}

		protected override Expression VisitMemberExpression(MemberExpression expression)
		{
			this.VisitExpression(expression.Expression);
			this._hqlExpression.AppendFormat(".{0}", expression.Member.Name);

			return expression;
		}

		protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
		{
			// In production code, handle this via method lookup tables.

			var supportedMethod = typeof(string).GetMethod("Contains");
			if(expression.Method.Equals(supportedMethod))
			{
				this._hqlExpression.Append("(");
				this.VisitExpression(expression.Object);
				this._hqlExpression.Append(" like '%'+");
				this.VisitExpression(expression.Arguments[0]);
				this._hqlExpression.Append("+'%')");
				return expression;
			}
			else
			{
				return base.VisitMethodCallExpression(expression); // throws
			}
		}

		protected override Expression VisitQuerySourceReferenceExpression(QuerySourceReferenceExpression expression)
		{
			this._hqlExpression.Append(expression.ReferencedQuerySource.ItemName);
			return expression;
		}

		#endregion
	}
}