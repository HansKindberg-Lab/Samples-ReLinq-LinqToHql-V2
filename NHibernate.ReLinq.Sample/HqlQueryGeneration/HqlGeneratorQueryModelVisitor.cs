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
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate.Type;
using Remotion.Collections;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using System.Text;
using Remotion.Data.Linq.Clauses.ResultOperators;

namespace NHibernate.ReLinq.Sample.HqlQueryGeneration
{
  public class HqlGeneratorQueryModelVisitor : QueryModelVisitorBase
  {
    public static CommandData GenerateHqlQuery (QueryModel queryModel)
    {
      var visitor = new HqlGeneratorQueryModelVisitor ();
      visitor.VisitQueryModel (queryModel);
      return visitor.GetHqlCommand();
    }

    // Instead of generating an HQL string, we could also use a NHibernate ASTFactory to generate IASTNodes.
    private readonly StringBuilder _hqlStringBuilder = new StringBuilder();
    private readonly ParameterAggregator _parameterAggregator = new ParameterAggregator();

    private bool _countSelected;
    private string _joinWhereCondition;

    public CommandData GetHqlCommand()
    {
      return new CommandData (_hqlStringBuilder.ToString(), _parameterAggregator.GetParameters());
    }

    public override void VisitQueryModel (QueryModel queryModel)
    {
      VisitResultOperators (queryModel.ResultOperators, queryModel);
      queryModel.SelectClause.Accept (this, queryModel);
      queryModel.MainFromClause.Accept (this, queryModel);

      SortBodyClauses(queryModel);
      VisitBodyClauses (queryModel.BodyClauses, queryModel);
    }

    public override void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index)
    {
      if (!(resultOperator is CountResultOperator))
        throw new NotSupportedException ("This query provider only supports Count() as a result operator.");

      _countSelected = true;

      base.VisitResultOperator (resultOperator, queryModel, index);
    }

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      _hqlStringBuilder.AppendFormat ("from {0} as {1} ", GetEntityName (fromClause), fromClause.ItemName);

      base.VisitMainFromClause (fromClause, queryModel);
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      if (_countSelected)
        _hqlStringBuilder.AppendFormat ("select cast(count({0}) as int) ", GetHqlExpression (selectClause.Selector)); // NH's count returns long, we need int
      else
        _hqlStringBuilder.AppendFormat ("select {0} ", GetHqlExpression (selectClause.Selector));

      base.VisitSelectClause (selectClause, queryModel);
    }

    public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
    {
      _hqlStringBuilder.AppendFormat ("where {0} ", GetHqlExpression (whereClause.Predicate));
      if (_joinWhereCondition != null)
        _hqlStringBuilder.AppendFormat ("and {0} ", _joinWhereCondition);


      base.VisitWhereClause (whereClause, queryModel, index);
    }

    public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
    {
      _hqlStringBuilder.Append ("order by ");

      bool first = true;
      foreach (var ordering in orderByClause.Orderings)
      {
        if (!first)
          _hqlStringBuilder.Append (", ");

        _hqlStringBuilder.Append (GetHqlExpression (ordering.Expression));

        first = false;
      }

      _hqlStringBuilder.Append (" ");

      base.VisitOrderByClause (orderByClause, queryModel, index);
    }

    public override void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, int index)
    {
      // HQL joins work differently, need to simulate using a cross join with a where condition
      _hqlStringBuilder.AppendFormat (", {0} as {1} ", 
          GetEntityName (joinClause), 
          joinClause.ItemName);
      _joinWhereCondition = string.Format (
          "({0} = {1})",
          GetHqlExpression (joinClause.OuterKeySelector), 
          GetHqlExpression (joinClause.InnerKeySelector));

      base.VisitJoinClause (joinClause, queryModel, index);
    }

    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      _hqlStringBuilder.AppendFormat (", {0} as {1} ",
          GetEntityName (fromClause),
          fromClause.ItemName);

      base.VisitAdditionalFromClause (fromClause, queryModel, index);
    }

    public override void VisitGroupJoinClause (GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
    {
      throw new NotSupportedException ("This query provider does not support join ... into ...");
    }

    private void SortBodyClauses (QueryModel queryModel)
    {
      var bodyClauseList = new List<IBodyClause> (queryModel.BodyClauses);
      bodyClauseList.Sort (CompareBodyClauses);
      queryModel.BodyClauses.Clear ();
      foreach (var bodyClause in bodyClauseList)
        queryModel.BodyClauses.Add (bodyClause);
    }

    private int CompareBodyClauses (IBodyClause left, IBodyClause right)
    {
      var leftPriority = GetBodyClausePriority (left);
      var rightPriority = GetBodyClausePriority (right);

      return leftPriority.CompareTo (rightPriority);
    }

    private int GetBodyClausePriority (IBodyClause bodyClause)
    {
      if (bodyClause is AdditionalFromClause)
        return 0;
      else if (bodyClause is JoinClause)
        return 1;
      else if (bodyClause is WhereClause)
        return 2;
      else if (bodyClause is OrderByClause)
        return 4;
      else
        throw new NotSupportedException ("Body clause type '" + bodyClause.GetType () + "' is not supported by this query provider.");
    }

    private string GetEntityName (IQuerySource querySource)
    {
      return NHibernateUtil.Entity (querySource.ItemType).Name;
    }

    private string GetHqlExpression (Expression expression)
    {
      return HqlGeneratorExpressionTreeVisitor.GetHqlExpression (expression, _parameterAggregator);
    }
  }
}