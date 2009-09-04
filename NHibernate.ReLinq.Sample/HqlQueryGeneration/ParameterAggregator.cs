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
using System.Collections.Generic;

namespace NHibernate.ReLinq.Sample.HqlQueryGeneration
{
  public class ParameterAggregator
  {
    private readonly List<NamedParameter> _parameters = new List<NamedParameter> ();

    public NamedParameter AddParameter (object value)
    {
      var parameter = new NamedParameter ("p" + (_parameters.Count + 1), value);
      _parameters.Add (parameter);
      return parameter;
    }

    public NamedParameter[] GetParameters ()
    {
      return _parameters.ToArray ();
    }
  }
}