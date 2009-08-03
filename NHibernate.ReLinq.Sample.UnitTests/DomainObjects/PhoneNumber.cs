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
using Remotion.Collections;
using Remotion.Diagnostics.ToText;

namespace NHibernate.ReLinq.Sample.UnitTests.DomainObjects
{
  public class PhoneNumber : IToTextConvertible
  {
    public virtual Guid NHibernateId { get; protected set; }
    public virtual string CountryCode { get; set; }
    public virtual string AreaCode { get; set; }
    public virtual string Number { get; set; }
    public virtual string Extension { get; set; }
    public virtual Person Person { get; set; }


    public static PhoneNumber NewObject()
    {
      return new PhoneNumber();
    }

    public static PhoneNumber NewObject(string CountryCode, string AreaCode, string Number, string Extension, Person person)
    {
      var phoneNumber = NewObject ();
      phoneNumber.CountryCode = CountryCode;
      phoneNumber.AreaCode = AreaCode;
      phoneNumber.Number = Number;
      phoneNumber.Extension = Extension;

      if (person != null) {
        person.AddPhoneNumber (phoneNumber);
      }

      return phoneNumber;
    }


    public virtual void SetPerson (Person person)
    {
      if (Person != null)
      {
        Person.RemovePhoneNumber (this);
      }
      Person = person;
    }


    #region CompoundValueEqualityComparer
    private static readonly CompoundValueEqualityComparer<PhoneNumber> _equalityComparer =
        new CompoundValueEqualityComparer<PhoneNumber> (a => new object[] {
                                                                              a.CountryCode, a.AreaCode, a.AreaCode, a.Number, a.Extension
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
      toTextBuilder.ib<PhoneNumber> ().e (CountryCode).e (AreaCode).e (Number).e (Extension).e(Person.FirstName).e(Person.Surname).ie ();
    }

    public override string ToString ()
    {
      var ttb = To.String;
      ToText (ttb);
      return ttb.ToString ();
    }
    #endregion

  }
}