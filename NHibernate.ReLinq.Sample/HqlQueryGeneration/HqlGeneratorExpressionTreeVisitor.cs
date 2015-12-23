// This file is part of NHibernate.ReLinq an NHibernate (www.nhibernate.org) Linq-provider.
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// NHibernate.ReLinq is based on re-motion re-linq (http://www.re-motion.org/).
// 
// NHibernate.ReLinq is free software: you can redistribute it and/or modify
// it under the terms of the Lesser GNU General Public License as published by
// the Free Software Foundation, either version 2.1 of the License, or
// (at your option) any later version.
// 
// NHibernate.ReLinq is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// Lesser GNU General Public License for more details.
// 
// You should have received a copy of the Lesser GNU General Public License
// along with NHibernate.ReLinq.  If not, see http://www.gnu.org/licenses/.
// 

using System;
using System.Linq.Expressions;
using System.Text;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;

namespace NHibernate.ReLinq.Sample.HqlQueryGeneration
{
  public class HqlGeneratorExpressionTreeVisitor : ThrowingExpressionTreeVisitor
  {
    public static string GetHqlExpression (Expression linqExpression, ParameterAggregator parameterAggregator)
    {
      var visitor = new HqlGeneratorExpressionTreeVisitor (parameterAggregator);
      visitor.VisitExpression (linqExpression);
      return visitor.GetHqlExpression ();
    }

    private readonly StringBuilder _hqlExpression = new StringBuilder ();
    private readonly ParameterAggregator _parameterAggregator;

    private HqlGeneratorExpressionTreeVisitor (ParameterAggregator parameterAggregator)
    {
      _parameterAggregator = parameterAggregator;
    }

    public string GetHqlExpression ()
    {
      return _hqlExpression.ToString ();
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      _hqlExpression.Append (expression.ReferencedQuerySource.ItemName);
      return expression;
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      _hqlExpression.Append ("(");

      VisitExpression (expression.Left);

      // In a more complex LINQ provider, handle this via lookup tables.
      switch (expression.NodeType)
      {
        case ExpressionType.Equal:
          _hqlExpression.Append (" = ");
          break;

        case ExpressionType.AndAlso:
        case ExpressionType.And:
          _hqlExpression.Append (" and ");
          break;
        
        case ExpressionType.OrElse:
        case ExpressionType.Or:
          _hqlExpression.Append (" or ");
          break;

        default:
          base.VisitBinaryExpression (expression);
          break;
      }

      VisitExpression (expression.Right);
      _hqlExpression.Append (")");

      return expression;
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      VisitExpression (expression.Expression);
      _hqlExpression.AppendFormat (".{0}", expression.Member.Name);

      return expression;
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      var namedParameter = _parameterAggregator.AddParameter (expression.Value);
      _hqlExpression.AppendFormat (":{0}", namedParameter.Name);

      return expression;
    }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      // In a more complex LINQ provider, handle this via method lookup tables.

      var supportedMethod = typeof (string).GetMethod ("Contains");
      if (expression.Method.Equals (supportedMethod))
      {
        _hqlExpression.Append ("(");
        VisitExpression (expression.Object);
        _hqlExpression.Append (" like '%'+");
        VisitExpression (expression.Arguments[0]);
        _hqlExpression.Append ("+'%')");
        return expression;
      }
      else
      {
        return base.VisitMethodCallExpression (expression);
      }
    }

    // Called when a LINQ expression type is not handled above.
    protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
    {
      string itemText = FormatUnhandledItem(unhandledItem);
      var message = string.Format ("The expression '{0}' (type: {1}) is not supported by this LINQ provider.", itemText, typeof (T));
      return new NotSupportedException (message);
    }

    private string FormatUnhandledItem<T>(T unhandledItem)
    {
      var itemAsExpression = unhandledItem as Expression;
      return itemAsExpression != null ? FormattingExpressionTreeVisitor.Format (itemAsExpression) : unhandledItem.ToString ();
    }
  }
}