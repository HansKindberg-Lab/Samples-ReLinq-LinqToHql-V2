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
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using System.Linq;

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
    private readonly QueryParts _queryParts = new QueryParts ();
    private readonly ParameterAggregator _parameterAggregator = new ParameterAggregator();

    public CommandData GetHqlCommand()
    {
      return new CommandData (_queryParts.BuildHQLString(), _parameterAggregator.GetParameters ());
    }

    public override void VisitQueryModel (QueryModel queryModel)
    {
      queryModel.SelectClause.Accept (this, queryModel);
      queryModel.MainFromClause.Accept (this, queryModel);
      VisitBodyClauses (queryModel.BodyClauses, queryModel);
      VisitResultOperators (queryModel.ResultOperators, queryModel);
    }

    public override void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index)
    {
      if (!(resultOperator is CountResultOperator))
        throw new NotSupportedException ("This query provider only supports Count() as a result operator.");

      _queryParts.SelectPart = string.Format ("cast(count({0}) as int)", _queryParts.SelectPart);

      base.VisitResultOperator (resultOperator, queryModel, index);
    }

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      _queryParts.AddFromPart (fromClause);

      base.VisitMainFromClause (fromClause, queryModel);
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      _queryParts.SelectPart = GetHqlExpression (selectClause.Selector);

      base.VisitSelectClause (selectClause, queryModel);
    }

    public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
    {
       _queryParts.AddWherePart (GetHqlExpression (whereClause.Predicate));

      base.VisitWhereClause (whereClause, queryModel, index);
    }

    public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
    {
      _queryParts.AddOrderByPart (orderByClause.Orderings.Select (o => GetHqlExpression (o.Expression)));

      base.VisitOrderByClause (orderByClause, queryModel, index);
    }

    public override void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, int index)
    {
      // HQL joins work differently, need to simulate using a cross join with a where condition

      _queryParts.AddFromPart (joinClause);
      _queryParts.AddWherePart (
          "({0} = {1})",
          GetHqlExpression (joinClause.OuterKeySelector), 
          GetHqlExpression (joinClause.InnerKeySelector));

      base.VisitJoinClause (joinClause, queryModel, index);
    }

    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      _queryParts.AddFromPart (fromClause);

      base.VisitAdditionalFromClause (fromClause, queryModel, index);
    }

    public override void VisitGroupJoinClause (GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
    {
      throw new NotSupportedException ("This query provider does not support join ... into ...");
    }

    private string GetHqlExpression (Expression expression)
    {
      return HqlGeneratorExpressionTreeVisitor.GetHqlExpression (expression, _parameterAggregator);
    }
  }
}