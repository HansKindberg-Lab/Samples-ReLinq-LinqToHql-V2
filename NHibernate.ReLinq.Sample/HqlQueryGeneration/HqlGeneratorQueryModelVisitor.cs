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

using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using System.Text;

namespace NHibernate.ReLinq.Sample.HqlQueryGeneration
{
  public class HqlGeneratorQueryModelVisitor : QueryModelVisitorBase
  {
    public static string GenerateHqlQuery (QueryModel queryModel)
    {
      var visitor = new HqlGeneratorQueryModelVisitor ();
      visitor.VisitQueryModel (queryModel);
      return visitor.GetHqlString();
    }

    private StringBuilder _hqlStringBuilder;

    public string GetHqlString()
    {
      return _hqlStringBuilder.ToString();
    }

    public override void VisitQueryModel (QueryModel queryModel)
    {
      _hqlStringBuilder = new StringBuilder ();

      queryModel.SelectClause.Accept (this, queryModel);
      queryModel.MainFromClause.Accept (this, queryModel);
    }

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      var entityName = NHibernateUtil.Entity (fromClause.ItemType);
      _hqlStringBuilder.AppendFormat ("from {0} as {1} ", entityName.Name, fromClause.ItemName);

      base.VisitMainFromClause (fromClause, queryModel);
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      _hqlStringBuilder.AppendFormat ("select {0} ", HqlGeneratorExpressionTreeVisitor.GetHqlExpression (selectClause.Selector));

      base.VisitSelectClause (selectClause, queryModel);
    }
  }
}