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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq;

namespace NHibernate.ReLinq.Sample
{
  /// <summary>
  /// Provides the main entry point to a LINQ query.
  /// </summary>
  public class NHQueryable<T> : QueryableBase<T>
  {
    private static IQueryExecutor CreateExecutor (ISession session)
    {
      return new NHQueryExecutor (new HqlQueryGenerator (session));
    }
    
    // This constructor is called by our users, create a new IQueryExecutor.
    public NHQueryable (ISession session)
        : base (CreateExecutor (session))
    {
    }

    // This constructor is called indirectly by LINQ's query methods, just pass to base.
    public NHQueryable (IQueryProvider provider, Expression expression) : base(provider, expression)
    {
    }
  }
}