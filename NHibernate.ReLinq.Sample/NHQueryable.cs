//  This file is part of NHibernate.ReLinq.Sample a sample showing
//  the use of the open source re-linq library to implement a non-trivial 
//  Linq-provider, on the example of NHibernate (www.nhibernate.org).
//  Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
//  
//  NHibernate.ReLinq.Sample is based on re-motion re-linq (http://www.re-motion.org/).
//  
//  NHibernate.ReLinq.Sample is free software; you can redistribute it 
//  and/or modify it under the terms of the GNU Lesser General Public License 
//  version 3.0 as published by the Free Software Foundation.
//  
//  re-motion is distributed in the hope that it will be useful, 
//  but WITHOUT ANY WARRANTY; without even the implied warranty of 
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
//  GNU Lesser General Public License for more details.
//  
//  You should have received a copy of the GNU Lesser General Public License
//  along with NHibernate.ReLinq.Sample; if not, see http://www.gnu.org/licenses.
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
      return new NHQueryExecutor (session);
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