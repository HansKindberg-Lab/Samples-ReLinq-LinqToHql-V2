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
using Remotion.Collections;
using Remotion.Diagnostics.ToText;

namespace NHibernate.ReLinq.Sample.UnitTests.DomainObjects
{
  public class Location : IToTextConvertible
  {
    public virtual Guid NHibernateId { get; protected set; }
 
    public virtual string Street { get; set; }
    public virtual string No { get; set; }
    public virtual string City { get; set; }
    public virtual Country? Country { get; set; }
    public virtual int ZipCode { get; set; }

    public virtual Person Owner { get; set; }

    public static Location NewObject ()
    {
      return new Location();
    }

    public static Location NewObject (string Street, string No, Country Country, int ZipCode, string City)
    {
      var location = NewObject ();
      location.Street = Street;
      location.No = No;
      location.Country = Country;
      location.ZipCode = ZipCode;
      location.City = City;
      return location;
    }


    #region CompoundValueEqualityComparer

    private static readonly CompoundValueEqualityComparer<Location> _equalityComparer =
        new CompoundValueEqualityComparer<Location> (a => new object[] {
                                                                           a.Street, a.Country, a.City, a.ZipCode, a.No
                                                                       });

    public override int GetHashCode ()
    {
      return _equalityComparer.GetHashCode (this);
    }

    public override bool Equals (object obj)
    {
      return _equalityComparer.Equals (this, obj);
    }

    #endregion

    #region ToString-ToText

    public virtual void ToText (IToTextBuilder toTextBuilder)
    {
      toTextBuilder.ib<Location> ().e (Street).e (No).e (City).e (ZipCode).e (Country).ie ();
    }

    public override string ToString ()
    {
      var ttb = To.String;
      ToText (ttb);
      return ttb.ToString ();
    }

    #endregion

  }

  public enum Country
  {
    Austria = 0,
    Australia = 1,
    BurkinaFaso = 2
  }
}