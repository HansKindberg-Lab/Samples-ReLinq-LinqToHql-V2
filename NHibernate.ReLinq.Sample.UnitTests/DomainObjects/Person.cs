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
using Remotion.Collections;
using Remotion.Diagnostics.ToText;

namespace NHibernate.ReLinq.Sample.UnitTests.DomainObjects
{
  public class Person : IToTextConvertible
  {
    public virtual Guid NHibernateId { get; protected set; }
    public virtual string FirstName { get; set; }
    public virtual string Surname { get; set; }
    public virtual Location Location { get; set; }

    public virtual IList<PhoneNumber> PhoneNumbers { get; set; }


    public static Person NewObject()
    {
      var person = new Person();
      person.PhoneNumbers = new List<PhoneNumber> ();
      return person;  
    }

    public static Person NewObject (string FirstName, string Surname, Location Location)
    {
      var person = NewObject ();
      person.FirstName = FirstName;
      person.Surname = Surname;
      person.Location = Location;
      return person;
    }



    #region CompoundValueEqualityComparer

    private static readonly CompoundValueEqualityComparer<Person> _equalityComparer =
        new CompoundValueEqualityComparer<Person> (a => new object[] {
                                                                         a.FirstName, a.Surname, a.Location, ComponentwiseEqualsAndHashcodeWrapper.New (a.PhoneNumbers)
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
      toTextBuilder.ib<Person> ().e (FirstName).e (Surname).e (Location).e (PhoneNumbers).ie ();
    }

    public override string ToString ()
    {
      var ttb = To.String;
      ToText (ttb);
      return ttb.ToString ();
    }

    #endregion



    public virtual void AddPhoneNumber (PhoneNumber phoneNumber)
    {
      phoneNumber.Person = this;
      PhoneNumbers.Add (phoneNumber);
    }

    public virtual void RemovePhoneNumber (PhoneNumber phoneNumber)
    {
      PhoneNumbers.Remove (phoneNumber);
    }

  }
}