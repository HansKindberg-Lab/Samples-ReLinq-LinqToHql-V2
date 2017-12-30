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
using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.Clauses;
using Remotion.Text;

namespace NHibernate.ReLinq.Sample.HqlQueryGeneration
{
	public class QueryPartsAggregator
	{
		#region Constructors

		public QueryPartsAggregator ()
		{
			this.FromParts = new List<string>();
			this.WhereParts = new List<string>();
			this.OrderByParts = new List<string>();
		}

		#endregion

		#region Properties

		private List<string> FromParts { get; }
		private List<string> OrderByParts { get; }
		public string SelectPart { get; set; }
		private List<string> WhereParts { get; }

		#endregion

		#region Methods

		public void AddFromPart (IQuerySource querySource)
		{
			this.FromParts.Add (string.Format ("{0} as {1}", this.GetEntityName (querySource), querySource.ItemName));
		}

		public void AddOrderByPart (IEnumerable<string> orderings)
		{
			this.OrderByParts.Insert (0, SeparatedStringBuilder.Build (", ", orderings));
		}

		public void AddWherePart (string formatString, params object[] args)
		{
			this.WhereParts.Add (string.Format (formatString, args));
		}

		public string BuildHQLString ()
		{
			var stringBuilder = new StringBuilder();

			if(string.IsNullOrEmpty (this.SelectPart) || this.FromParts.Count == 0)
				throw new InvalidOperationException ("A query must have a select part and at least one from part.");

			stringBuilder.AppendFormat ("select {0}", this.SelectPart);
			stringBuilder.AppendFormat (" from {0}", SeparatedStringBuilder.Build (", ", this.FromParts));

			if(this.WhereParts.Count > 0)
				stringBuilder.AppendFormat (" where {0}", SeparatedStringBuilder.Build (" and ", this.WhereParts));

			if(this.OrderByParts.Count > 0)
				stringBuilder.AppendFormat (" order by {0}", SeparatedStringBuilder.Build (", ", this.OrderByParts));

			return stringBuilder.ToString();
		}

		private string GetEntityName (IQuerySource querySource)
		{
			return NHibernateUtil.Entity (querySource.ItemType).Name;
		}

		#endregion
	}
}