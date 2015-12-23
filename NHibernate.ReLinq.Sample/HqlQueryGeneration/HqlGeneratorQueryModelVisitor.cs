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

    public CommandData GetHqlCommand()
    {
      return new CommandData (_hqlStringBuilder.ToString(), _parameterAggregator.GetParameters());
    }

    public override void VisitQueryModel (QueryModel queryModel)
    {
      VisitResultOperators (queryModel.ResultOperators, queryModel);
      queryModel.SelectClause.Accept (this, queryModel);
      queryModel.MainFromClause.Accept (this, queryModel);
      VisitBodyClauses (queryModel.BodyClauses, queryModel);
    }

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      var entityName = NHibernateUtil.Entity (fromClause.ItemType);
      _hqlStringBuilder.AppendFormat ("from {0} as {1} ", entityName.Name, fromClause.ItemName);

      base.VisitMainFromClause (fromClause, queryModel);
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      if (_countSelected)
        _hqlStringBuilder.AppendFormat ("select cast(count(*) as int) "); // NH's count returns long, we need int
      else
        _hqlStringBuilder.AppendFormat ("select {0} ", GetHqlExpression (selectClause.Selector));

      base.VisitSelectClause (selectClause, queryModel);
    }

    public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
    {
      _hqlStringBuilder.AppendFormat ("where {0} ", GetHqlExpression (whereClause.Predicate));

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

    public override void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index)
    {
      if (!(resultOperator is CountResultOperator))
        throw new NotSupportedException ("This query provider only supports Count() as a result operator.");

      _countSelected = true;
      
      base.VisitResultOperator (resultOperator, queryModel, index);
    }

    private string GetHqlExpression (Expression expression)
    {
      return HqlGeneratorExpressionTreeVisitor.GetHqlExpression (expression, _parameterAggregator);
    }
  }
}