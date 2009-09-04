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
using System.Linq;
using NHibernate.ReLinq.Sample.HqlQueryGeneration;
using Remotion.Data.Linq;

namespace NHibernate.ReLinq.Sample
{
  // Called by re-linq when a query is to be executed.
  public class NHQueryExecutor : IQueryExecutor
  {
    private readonly ISession _session;

    public NHQueryExecutor (ISession session)
    {
      _session = session;
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
      var commandData = HqlGeneratorQueryModelVisitor.GenerateHqlQuery (queryModel);
      var query = commandData.CreateQuery (_session);
      return query.Enumerable<T> ();
    }
  }
}