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
using System.Linq;
using Remotion.Data.Linq;

namespace NHibernate.ReLinq.Sample
{
  // Called by re-linq when a query is to be executed.
  public class NHQueryExecutor : IQueryExecutor
  {
    private readonly HqlQueryGenerator _queryGenerator;

    public NHQueryExecutor (HqlQueryGenerator queryGenerator)
    {
      _queryGenerator = queryGenerator;
    }

    // Executes a query with a scalar result, i.e. a query that ends with a result operator such as Count, Sum, or Average.
    public T ExecuteScalar<T> (QueryModel queryModel)
    {
      return ExecuteCollection<T> (queryModel).Single();
    }

    // Executes a query with a single result object, i.e. a query that ends with a result operator such as First, Last, Single, Min, or Max.
    public T ExecuteSingle<T> (QueryModel queryModel, bool returnDefaultWhenEmpty)
    {
      return returnDefaultWhenEmpty ? ExecuteCollection<T> (queryModel).SingleOrDefault () : ExecuteCollection<T> (queryModel).Single ();
    }

    // Executes a query with a collection result.
    public IEnumerable<T> ExecuteCollection<T> (QueryModel queryModel)
    {
      var query = _queryGenerator.CreateQuery (queryModel);
      return query.Enumerable<T> ();
    }
  }
}