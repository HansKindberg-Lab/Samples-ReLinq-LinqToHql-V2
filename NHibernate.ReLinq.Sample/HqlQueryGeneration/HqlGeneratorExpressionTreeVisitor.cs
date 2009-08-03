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
using Remotion.Data.Linq.Parsing;

namespace NHibernate.ReLinq.Sample.HqlQueryGeneration
{
  public class HqlGeneratorExpressionTreeVisitor : ThrowingExpressionTreeVisitor
  {
    public static string GetHqlExpression (Expression linqExpression)
    {
      var visitor = new HqlGeneratorExpressionTreeVisitor ();
      visitor.VisitExpression (linqExpression);
      return visitor.GetHqlExpression ();
    }

    private readonly StringBuilder _hqlExpression = new StringBuilder ();

    public string GetHqlExpression ()
    {
      return _hqlExpression.ToString ();
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      _hqlExpression.Append (expression.ReferencedQuerySource.ItemName);
      return expression;
    }

    // Called when a LINQ expression type is not handled above.
    protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
    {
      var message = string.Format ("The expression type '{0}' is not supported by this LINQ provider.", typeof (T));
      return new NotSupportedException (message);
    }


  }
}