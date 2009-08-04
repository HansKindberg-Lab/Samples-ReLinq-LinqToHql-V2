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
using System.Text;
using Remotion.Data.Linq.Clauses;
using Remotion.Text;

namespace NHibernate.ReLinq.Sample.HqlQueryGeneration
{
  public class QueryPartsAggregator
  {
    public QueryPartsAggregator()
    {
      FromParts = new List<string> ();
      WhereParts = new List<string> ();
      OrderByParts = new List<string> ();
    }

    public string SelectPart { get; set; }
    private List<string> FromParts { get; set; }
    private List<string> WhereParts { get; set; }
    private List<string> OrderByParts { get; set; }

    public void AddFromPart (IQuerySource querySource)
    {
      FromParts.Add (string.Format ("{0} as {1}", GetEntityName (querySource), querySource.ItemName));
    }

    public void AddWherePart (string formatString, params object[] args)
    {
      WhereParts.Add (string.Format (formatString, args));
    }

    public void AddOrderByPart (IEnumerable<string> orderings)
    {
      OrderByParts.Insert (0, SeparatedStringBuilder.Build (", ", orderings));
    }

    public string BuildHQLString ()
    {
      var stringBuilder = new StringBuilder ();

      if (string.IsNullOrEmpty (SelectPart) || FromParts.Count == 0)
        throw new InvalidOperationException ("A query must have a select part and at least one from part.");

      stringBuilder.AppendFormat ("select {0}", SelectPart);
      stringBuilder.AppendFormat (" from {0}", SeparatedStringBuilder.Build (", ", FromParts));

      if (WhereParts.Count > 0)
        stringBuilder.AppendFormat (" where {0}", SeparatedStringBuilder.Build (" and ", WhereParts));

      if (OrderByParts.Count > 0)
        stringBuilder.AppendFormat (" order by {0}", SeparatedStringBuilder.Build (", ", OrderByParts));

      return stringBuilder.ToString ();
    }

    private string GetEntityName (IQuerySource querySource)
    {
      return NHibernateUtil.Entity (querySource.ItemType).Name;
    }
  }
}