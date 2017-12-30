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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;

namespace NHibernate.ReLinq.Sample.HqlQueryGeneration
{
	public class HqlGeneratorQueryModelVisitor : QueryModelVisitorBase
	{
		#region Fields

		private readonly ParameterAggregator _parameterAggregator = new ParameterAggregator();

		// Instead of generating an HQL string, we could also use a NHibernate ASTFactory to generate IASTNodes.
		private readonly QueryPartsAggregator _queryParts = new QueryPartsAggregator();

		#endregion

		#region Methods

		public static CommandData GenerateHqlQuery (QueryModel queryModel)
		{
			var visitor = new HqlGeneratorQueryModelVisitor();
			visitor.VisitQueryModel (queryModel);
			return visitor.GetHqlCommand();
		}

		public CommandData GetHqlCommand ()
		{
			return new CommandData (this._queryParts.BuildHQLString(), this._parameterAggregator.GetParameters());
		}

		private string GetHqlExpression (Expression expression)
		{
			return HqlGeneratorExpressionTreeVisitor.GetHqlExpression (expression, this._parameterAggregator);
		}

		public override void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
		{
			this._queryParts.AddFromPart (fromClause);

			base.VisitAdditionalFromClause (fromClause, queryModel, index);
		}

		public override void VisitGroupJoinClause (GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
		{
			throw new NotSupportedException ("Adding a join ... into ... implementation to the query provider is left to the reader for extra points.");
		}

		public override void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, int index)
		{
			// HQL joins work differently, need to simulate using a cross join with a where condition

			this._queryParts.AddFromPart (joinClause);
			this._queryParts.AddWherePart (
					"({0} = {1})",
					this.GetHqlExpression (joinClause.OuterKeySelector),
					this.GetHqlExpression (joinClause.InnerKeySelector));

			base.VisitJoinClause (joinClause, queryModel, index);
		}

		public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
		{
			this._queryParts.AddFromPart (fromClause);

			base.VisitMainFromClause (fromClause, queryModel);
		}

		public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
		{
			this._queryParts.AddOrderByPart (orderByClause.Orderings.Select (o => this.GetHqlExpression (o.Expression)));

			base.VisitOrderByClause (orderByClause, queryModel, index);
		}

		public override void VisitQueryModel (QueryModel queryModel)
		{
			queryModel.SelectClause.Accept (this, queryModel);
			queryModel.MainFromClause.Accept (this, queryModel);
			this.VisitBodyClauses (queryModel.BodyClauses, queryModel);
			this.VisitResultOperators (queryModel.ResultOperators, queryModel);
		}

		public override void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index)
		{
			if(resultOperator is CountResultOperator)
				this._queryParts.SelectPart = string.Format ("cast(count({0}) as int)", this._queryParts.SelectPart);
			else
				throw new NotSupportedException ("Only Count() result operator is showcased in this sample. Adding Sum, Min, Max is left to the reader.");

			base.VisitResultOperator (resultOperator, queryModel, index);
		}

		public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
		{
			this._queryParts.SelectPart = this.GetHqlExpression (selectClause.Selector);

			base.VisitSelectClause (selectClause, queryModel);
		}

		public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
		{
			this._queryParts.AddWherePart (this.GetHqlExpression (whereClause.Predicate));

			base.VisitWhereClause (whereClause, queryModel, index);
		}

		#endregion
	}
}