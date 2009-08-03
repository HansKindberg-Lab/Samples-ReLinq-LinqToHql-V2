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

namespace NHibernate.ReLinq.Sample.HqlQueryGeneration
{
  public class CommandData
  {
    public CommandData (string statement, NamedParameter[] namedParameters)
    {
      Statement = statement;
      NamedParameters = namedParameters;
    }

    public string Statement { get; private set; }
    public NamedParameter[] NamedParameters { get; private set; }

    public IQuery CreateQuery (ISession session)
    {
      var query = session.CreateQuery (Statement);

      foreach (var parameter in NamedParameters)
        query.SetParameter (parameter.Name, parameter.Value);

      return query;
    }
  }
}