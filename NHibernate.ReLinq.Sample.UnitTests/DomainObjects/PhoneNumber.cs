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