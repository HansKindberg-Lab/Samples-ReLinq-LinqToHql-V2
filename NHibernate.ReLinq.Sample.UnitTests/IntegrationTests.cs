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
using System.Linq;
using NHibernate.Cfg;
using NHibernate.ReLinq.Sample.UnitTests.DomainObjects;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework.SyntaxHelpers;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing.Structure;

namespace NHibernate.ReLinq.Sample.UnitTests
{
  [TestFixture]
  public class IntegrationTests
  { 
    private ISessionFactory _sessionFactory;
    private Configuration _configuration;
    private SchemaExport _schemaExport;
    private Location _location;
    private Location _location2;
    private Person _person;
    private Person _person2;
    private Person _person3;
    private PhoneNumber _phoneNumber;
    private PhoneNumber _phoneNumber2;
    private PhoneNumber _phoneNumber3;
    private PhoneNumber _phoneNumber4;

    [TestFixtureSetUp]
    public void TestFixtureSetUp ()
    {
      _configuration = new Configuration ();
      _configuration.Configure ();

      // Add all NHibernate mapping embedded config resources (i.e. all "*.hbm.xml") from this assembly.
      _configuration.AddAssembly (GetType ().Assembly);

      _sessionFactory = _configuration.BuildSessionFactory ();
      _schemaExport = new SchemaExport (_configuration);
    }


    [SetUp]
    public void Setup ()
    {
      // Create DB tables
      _schemaExport.Execute (false, true, false);

      SetupTestData();
    }

    [TearDown]
    public void TearDown ()
    {
      // Drop DB tables
      _schemaExport.Drop (false, true);
    }


    [Test]
    public void SelectAllTest ()
    {
      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in new NHQueryable<PhoneNumber> (session)
                    select pn;

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] {_phoneNumber, _phoneNumber2, _phoneNumber3, _phoneNumber4}));

        var nhibernateQuery = new HqlQueryGenerator (session).CreateQuery (new QueryParser ().GetParsedQuery (query.Expression));
        Assert.That (nhibernateQuery.QueryString, Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "));
      }
    }


    //[Test]
    //public void SelectFromPhoneNumberWithWhereTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber = PhoneNumber.NewObject ("11111", "2-111", "3-111111", "4-11", person2);
    //  var phoneNumber3 = PhoneNumber.NewObject ("11111", "2-333", "3-333333", "4-33", person);
    //  var phoneNumber4 = PhoneNumber.NewObject ("11111", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from pn in QueryFactory.CreateLinqQuery<PhoneNumber> (session)
    //                where pn.CountryCode == "11111"
    //                select pn;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select pn from PhoneNumber as pn where (pn.CountryCode = :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (phoneNumber, phoneNumber3, phoneNumber4)));
    //  }
    //}



    //[Test]
    //public void ImplicitJoinTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where p.Location.Street == "Personenstraﬂe"
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p join p.Location as j0 where (j0.Street = :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person2)));
    //  }
    //}




    //[Test]
    //public void ComplexImplicitJoinTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber = PhoneNumber.NewObject ("1-1", "2-111", "3-111111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from pn in QueryFactory.CreateLinqQuery<PhoneNumber> (session)
    //                where pn.Person.Location.Street == "Gassengasse"
    //                select pn;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select pn from PhoneNumber as pn join pn.Person as j0 join j0.Location as j1 where (j1.Street = :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (phoneNumber2, phoneNumber3)));
    //  }
    //}



    //[Test]
    //public void OrderbyTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from pn in QueryFactory.CreateLinqQuery<PhoneNumber> (session)
    //                orderby pn.Number descending
    //                select pn;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select pn from PhoneNumber as pn order by pn.Number desc"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (phoneNumber4, phoneNumber3, phoneNumber1, phoneNumber2)));
    //  }
    //}


    //[Test]
    //public void ComplexWhereTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where p.FirstName == "Max"
    //                where p.Surname == "Muster"
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p where ((p.FirstName = :p1) and (p.Surname = :p2))"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person2)));
    //  }
    //}



    //[Test]
    //public void ComplexWhereTest2 ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where (p.FirstName == "Max" && p.Surname == "Muster") || p.Surname == "Mauser"
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p where (((p.FirstName = :p1) and (p.Surname = :p2)) or (p.Surname = :p3))"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person2, person3)));
    //  }
    //}

    //[Test]
    //public void ComplexWhereTest3 ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where (p.FirstName == "Max" && p.Surname == "Muster") ||
    //                      (p.Location.Street == "Gassengasse" && p.Surname == "Mauser")
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p join p.Location as j0 where (((p.FirstName = :p1) and (p.Surname = :p2)) or ((j0.Street = :p3) and (p.Surname = :p4)))"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person2, person3)));
    //  }
    //}


    //[Test]
    //public void ToUpperTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where p.FirstName.ToUpper () == "MAX"
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p where (upper(p.FirstName) = :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person2)));
    //  }
    //}

    //[Test]
    //public void ToLowerTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where p.FirstName.ToLower () == "max"
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p where (lower(p.FirstName) = :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person2)));
    //  }
    //}

    //[Test]
    //public void ContainsTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where p.FirstName.Contains ("M")
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p where (p.FirstName like :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person2, person3)));
    //  }
    //}

    //[Test]
    //public void WhereObjectReferenceTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from pn in QueryFactory.CreateLinqQuery<PhoneNumber> (session)
    //                where pn.Person == person2
    //                select pn;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select pn from PhoneNumber as pn where (pn.Person = :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (phoneNumber1, phoneNumber4)));
    //  }
    //}

    //[Test]
    //public void WhereWithImplicitJoinTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from pn in QueryFactory.CreateLinqQuery<PhoneNumber> (session)
    //                where (pn.Person.FirstName.Contains ("M") || pn.Person.Location.Country == Country.Australia) &&
    //                      pn.AreaCode.Contains ("2-")
    //                select pn;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select pn from PhoneNumber as pn join pn.Person as j0 join j0.Location as j1 where (((j0.FirstName like :p1) or (j1.Country = :p2)) and (pn.AreaCode like :p3))"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (phoneNumber1, phoneNumber2, phoneNumber3, phoneNumber4)));
    //  }
    //}

    //[Test]
    //public void WhereWithIsNullTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject (null, "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where p.FirstName == null
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p where p.FirstName is null"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person3)));
    //  }
    //}

    //[Test]
    //public void WhereWithIsNotNullTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber1 = PhoneNumber.NewObject ("1-1", "2-111", "3-31111", "4-11", person2);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", person1);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", person2);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where p.FirstName != null
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);

    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p where p.FirstName is not null"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person1, person2, person3)));
    //  }
    //}



    //[Test]
    //public void FromFromTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  var phoneNumber = PhoneNumber.NewObject ("1-1", "2-111", "3-111111", "4-11", null);
    //  var phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", person1);
    //  var phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", null);
    //  var phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", null);
    //  NHibernateSaveOrUpdate (location, location2, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                from pn in QueryFactory.CreateLinqQuery<PhoneNumber> (session)
    //                where pn.Person == p
    //                select pn;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select pn from Person as p, PhoneNumber as pn where ((pn.Person is null and p is null) or pn.Person = p)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (phoneNumber2)));
    //  }
    //}


    //[Test]
    //public void DistinctTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location);
    //  var person2 = Person.NewObject ("Max", "Muster", location);

    //  NHibernateSaveOrUpdate (location, person1, person2);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = (from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                 select p.Location);

    //    var queryDistinct = (from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                         select p.Location).Distinct();

    //    var result = query.ToList ();
    //    var resultDistinct = queryDistinct.ToList ();
    //    CommandData commandData = GetCommandData (queryDistinct);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select distinct j0 from Person as p join p.Location as j0"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (location, location)));
    //    Assert.That (resultDistinct, Is.EquivalentTo (ListMother.New (location)));
    //  }
    //}


    //[Test]
    //public void CountTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  NHibernateSaveOrUpdate (location, location2);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var result = (from l in QueryFactory.CreateLinqQuery<Location> (session) select l).Count();
    //    Assert.That (result, Is.EqualTo(2));
    //  }
    //}


    //[Test]
    //public void SubqueryTest ()
    //{
    //  var location = Location.NewObject ("Oerson", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Oerson", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  NHibernateSaveOrUpdate (location, location2, person1);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where (from l in QueryFactory.CreateLinqQuery<Location> (session) where l.Street == p.Surname select l).Count () == 2
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p where ((select count(*) from Location as l where ((l.Street is null and p.Surname is null) or l.Street = p.Surname)) = :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person1)));
    //  }
    //}


    //[Test]
    //public void SubqueryWithContainsTest ()
    //{
    //  var location = Location.NewObject ("Oerson", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Oerson", "22", Country.Australia, 12345, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location);
    //  NHibernateSaveOrUpdate (location, location2, person1);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where (from l in QueryFactory.CreateLinqQuery<Location> (session) where l.Street.Contains ("Oerson") select l).Count () == 2
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Person as p where ((select count(*) from Location as l where (l.Street like :p1)) = :p2)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person1)));
    //  }
    //}






    //[Test]
    //public void GreaterThanTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  NHibernateSaveOrUpdate (location, location2);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from l in QueryFactory.CreateLinqQuery<Location> (session)
    //                where l.ZipCode > 20000
    //                select l;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select l from Location as l where (l.ZipCode > :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (location)));
    //  }
    //}

    //[Test]
    //public void LessThanTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  NHibernateSaveOrUpdate (location, location2);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from l in QueryFactory.CreateLinqQuery<Location> (session)
    //                where l.ZipCode < 20000
    //                select l;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select l from Location as l where (l.ZipCode < :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (location2)));
    //  }
    //}

    //[Test]
    //public void LessThanOrEqualTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 12344, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
    //  NHibernateSaveOrUpdate (location, location2);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from l in QueryFactory.CreateLinqQuery<Location> (session)
    //                where l.ZipCode <= 12345
    //                select l;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select l from Location as l where (l.ZipCode <= :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (location, location2)));
    //  }
    //}

    //[Test]
    //public void GreaterThanOrEqualTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 100000, "Sydney");
    //  NHibernateSaveOrUpdate (location, location2);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from l in QueryFactory.CreateLinqQuery<Location> (session)
    //                where l.ZipCode >= 99999
    //                select l;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select l from Location as l where (l.ZipCode >= :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (location, location2)));
    //  }
    //}

    //[Test]
    //public void NotEqualTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 100000, "Sydney");
    //  var location3 = Location.NewObject ("Gassengasse", "22", Country.Australia, 100001, "Sydney");
    //  NHibernateSaveOrUpdate (location, location2, location3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from l in QueryFactory.CreateLinqQuery<Location> (session)
    //                where l.ZipCode != 99999
    //                select l;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select l from Location as l where (l.ZipCode is null or l.ZipCode <> :p1)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (location2, location3)));
    //  }
    //}


    //[Test]
    //public void ComplexWhereArithmeticTest ()
    //{
    //  var location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 3, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12, "Sydney");
    //  var location3 = Location.NewObject ("Gassengasse", "22", Country.Australia, 100, "Sydney");
    //  NHibernateSaveOrUpdate (location, location2, location3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from l in QueryFactory.CreateLinqQuery<Location> (session)
    //                where ((((((l.ZipCode * l.ZipCode) - 2) / 7) + l.ZipCode)) == 4)
    //                select l;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    To.ConsoleLine.e (commandData.Statement);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select l from Location as l where (((((l.ZipCode * l.ZipCode) - :p1) / :p2) + l.ZipCode) = :p3)"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (location)));
    //  }
    //}

    //[Test]
    //public void ComplexTest1 ()
    //{
    //  var location1 = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 3, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12, "Sydney");
    //  var location3 = Location.NewObject ("Gassengasse", "22", Country.Australia, 100, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Max", "Muster", location1);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  NHibernateSaveOrUpdate (location1, location2, location3, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from l in QueryFactory.CreateLinqQuery<Location> (session)
    //                from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where ((((((l.ZipCode * l.ZipCode) - 2) / 7) + l.ZipCode)) == 4)
    //                      && p.Surname.Contains ("M") && p.Location == l
    //                select l;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select l from Location as l, Person as p where (((((((l.ZipCode * l.ZipCode) - :p1) / :p2) + l.ZipCode) = :p3) and (p.Surname like :p4)) and ((p.Location is null and l is null) or p.Location = l))"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (location1)));
    //  }
    //}

    //[Test]
    //public void ComplexTest2 ()
    //{
    //  var location1 = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 3, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12, "Sydney");
    //  var location3 = Location.NewObject ("Gassengasse", "22", Country.Australia, 100, "Sydney");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location2);
    //  var person2 = Person.NewObject ("Piea", "Muster", location1);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location2);
    //  NHibernateSaveOrUpdate (location1, location2, location3, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from l in QueryFactory.CreateLinqQuery<Location> (session)
    //                from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where (((((((l.ZipCode * l.ZipCode) - 2) / 7) + l.ZipCode)) == 4)
    //                       && p.Surname.Contains ("M") && p.Location == l) ||
    //                      (p.FirstName.Contains("Pie") && p.Location == l)
    //                select l;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select l from Location as l, Person as p where ((((((((l.ZipCode * l.ZipCode) - :p1) / :p2) + l.ZipCode) = :p3) and (p.Surname like :p4)) and ((p.Location is null and l is null) or p.Location = l)) or ((p.FirstName like :p5) and ((p.Location is null and l is null) or p.Location = l)))"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (location1, location2)));
    //  }
    //}


    //[Test]
    //public void ComplexTest3 ()
    //{
    //  var location1 = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 3, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12, "Sydney");
    //  var location3 = Location.NewObject ("Gassengasse", "22", Country.Austria, 100, "Vienna");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location1);
    //  var person2 = Person.NewObject ("Piea", "Muster", location3);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location3);

    //  location1.Owner = person1;
    //  location2.Owner = person2;
    //  location3.Owner = person3;

    //  NHibernateSaveOrUpdate (location1, location2, location3, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = from l in QueryFactory.CreateLinqQuery<Location> (session)
    //                from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                where (l.Owner == p) && (p.Location == l)
    //                select p;

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select p from Location as l, Person as p where (((l.Owner is null and p is null) or l.Owner = p) and ((p.Location is null and l is null) or p.Location = l))"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person1, person3)));
    //  }
    //}

    //[Test]
    //public void ComplexTest4 ()
    //{
    //  var location1 = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 3, "Ouagadougou");
    //  var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12, "Sydney");
    //  var location3 = Location.NewObject ("Gassengasse", "22", Country.Austria, 100, "Vienna");
    //  var person1 = Person.NewObject ("Pierre", "Oerson", location1);
    //  var person2 = Person.NewObject ("Piea", "Muster", location3);
    //  var person3 = Person.NewObject ("Minny", "Mauser", location3);
    //  var person4 = Person.NewObject ("Dieter", "Dummy", location3);

    //  location1.Owner = person1;
    //  location2.Owner = person2;
    //  location3.Owner = person3;

    //  NHibernateSaveOrUpdate (location1, location2, location3, person1, person2, person3);

    //  using (ISession session = _sessionFactory.OpenSession ())
    //  {
    //    var query = (from l in QueryFactory.CreateLinqQuery<Location> (session)
    //                 from p in QueryFactory.CreateLinqQuery<Person> (session)
    //                 where (((l.Owner == p) && (p.Location == l)) && 
    //                        (l.City.Contains ("na") || p.Surname.Contains ("Oe"))) || p == person2
    //                 select p).Distinct();

    //    var result = query.ToList ();
    //    CommandData commandData = GetCommandData (query);
    //    Assert.That (commandData.Statement, Is.EqualTo ("select distinct p from Location as l, Person as p where (((((l.Owner is null and p is null) or l.Owner = p) and ((p.Location is null and l is null) or p.Location = l)) and ((l.City like :p1) or (p.Surname like :p2))) or (p = :p3))"));
    //    Assert.That (result, Is.EquivalentTo (ListMother.New (person1, person2, person3)));
    //  }
    //}

    private void SetupTestData ()
    {
      _location = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
      _location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
      _person = Person.NewObject ("Pierre", "Oerson", _location2);
      _person2 = Person.NewObject ("Max", "Muster", _location);
      _person3 = Person.NewObject ("Minny", "Mauser", _location2);
      _phoneNumber = PhoneNumber.NewObject ("1-1", "2-111", "3-111111", "4-11", _person2);
      _phoneNumber2 = PhoneNumber.NewObject ("1-2", "2-222", "3-22222", "4-22", _person);
      _phoneNumber3 = PhoneNumber.NewObject ("1-3", "2-333", "3-333333", "4-33", _person);
      _phoneNumber4 = PhoneNumber.NewObject ("1-4", "2-444", "3-44444", "4-44444", _person2);
      NHibernateSaveOrUpdate (_location, _location2, _person, _person2, _person3);
    }

    private void NHibernateSaveOrUpdate (params object[] objectsToSave)
    {
      using (ISession session = _sessionFactory.OpenSession ())
      using (ITransaction transaction = session.BeginTransaction ())
      {
        foreach (var o in objectsToSave)
        {
          session.SaveOrUpdate (o);
        }
        transaction.Commit ();
      }
    }

  }
}