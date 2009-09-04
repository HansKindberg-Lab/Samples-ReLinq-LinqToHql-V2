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
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Cfg;
using NHibernate.ReLinq.Sample.HqlQueryGeneration;
using NHibernate.ReLinq.Sample.UnitTests.DomainObjects;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework.SyntaxHelpers;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
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
    private Person _person4;
    private PhoneNumber _phoneNumber;
    private PhoneNumber _phoneNumber2;
    private PhoneNumber _phoneNumber3;
    private PhoneNumber _phoneNumber4;
    private PhoneNumber _phoneNumber5;

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
      // Create NHibernate DB tables
      _schemaExport.Execute (false, true, false);

      SetupTestData();
    }

    [TearDown]
    public void TearDown ()
    {
      // Drop NHibernate DB tables
      _schemaExport.Drop (false, true);
    }


    [Test]
    public void SelectFrom ()
    {
      // Implement VisitMainFromClause, VisitSelectClause
      // Implement VisitQuerySourceReferenceExpression

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in NHQueryFactory.Queryable<PhoneNumber> (session)
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString, Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn"));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _phoneNumber, _phoneNumber2, _phoneNumber3, _phoneNumber4, _phoneNumber5 }));
      }
    }

    [Test]
    public void SelectFromWhere ()
    {
      // Implement VisitWhereClause
      // Implement VisitBinaryExpression (Equal), VisitMemberExpression, VisitConstantExpression (+ ParameterAggregator, etc.)

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in NHQueryFactory.Queryable<PhoneNumber> (session)
                    where pn.CountryCode == "11111"
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn where (pn.CountryCode = :p1)"));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _phoneNumber, _phoneNumber3, _phoneNumber4, _phoneNumber5 }));
      }
    }

    [Test]
    public void SelectFromWhere_WithAndOr ()
    {
      // Implement VisitBinaryExpression (And/AndAlso/Or/OrElse)

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in NHQueryFactory.Queryable<PhoneNumber> (session)
                    where pn.CountryCode == "11111" || (pn.Person.FirstName == "Pierre" && pn.Person.Surname == "Oerson")
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString, 
            Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "
                + "where ((pn.CountryCode = :p1) or ((pn.Person.FirstName = :p2) and (pn.Person.Surname = :p3)))"));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _phoneNumber, _phoneNumber2, _phoneNumber3, _phoneNumber4, _phoneNumber5 }));
      }
    }


    [Test]
    public void SelectFromWhere_WithPlusMinus ()
    {
      // Implement VisitBinaryExpression (Add/Subtract)

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from l in NHQueryFactory.Queryable<Location> (session)
                    where ((l.ZipCode + 1) == 12346) || ((l.ZipCode - 99990) == 9)
                    select l;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select l from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Location as l "
                + "where (((l.ZipCode + :p1) = :p2) or ((l.ZipCode - :p3) = :p4))"));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _location, _location2 }));
      }
    }


    [Test]
    public void SelectFromWhere_WithContains ()
    {
      // Implement VisitMethodCallExpression

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from p in NHQueryFactory.Queryable<Person> (session)
                    where p.Surname.Contains (p.FirstName)
                    select p;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select p from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p where (p.Surname like '%'+p.FirstName+'%')"));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _person4 }));
      }
    }

    [Test]
    public void SelectFromOrderBy ()
    {
      // Implement VisitOrderByClause

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in NHQueryFactory.Queryable<PhoneNumber> (session)
                    orderby pn.Number, pn.CountryCode
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn order by pn.Number, pn.CountryCode"));

        var result = query.ToList ();
        Assert.That (result, Is.EqualTo (new[] { _phoneNumber, _phoneNumber2, _phoneNumber4, _phoneNumber3, _phoneNumber5 }));
      }
    }

    [Test]
    public void SelectFromCount ()
    {
      // Implement VisitResultOperator

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in NHQueryFactory.Queryable<PhoneNumber> (session)
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, MakeExpression (query, q => q.Count()));
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select cast(count(pn) as int) from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn"));

        var result = query.Count();
        Assert.That (result, Is.EqualTo (5));
      }
    }

    [Test]
    public void SelectFromJoin()
    {
      // Implement VisitJoinClause

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in NHQueryFactory.Queryable<PhoneNumber> (session)
                    join p in NHQueryFactory.Queryable<Person> (session) on pn.Person equals p
                    where pn.CountryCode == "22222"
                    select p;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select p from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn, "
                + "NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p where (pn.Person = p) and (pn.CountryCode = :p1)"));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _person }));
      }
    }

    [Test]
    public void SelectFromFromWhere ()
    {
      // Implement VisitAdditionalFromClause

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in NHQueryFactory.Queryable<PhoneNumber> (session)
                    from p in NHQueryFactory.Queryable<Person> (session)
                    where pn.Person == p && pn.CountryCode == "22222"
                    select p;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select p from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn, "
                + "NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p where ((pn.Person = p) and (pn.CountryCode = :p1))"));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _person }));
      }
    }

    [Test]
    public void SelectFromWhereOrderByFrom_ClauseOrder ()
    {
      // Implicitly sorted via QueryPartsAggregator class

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from p in NHQueryFactory.Queryable<Person> (session)
                    where p.Surname == "Oerson"
                    orderby p.Surname
                    join pn in NHQueryFactory.Queryable<PhoneNumber> (session) on p equals pn.Person
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString, 
            Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p, "
            + "NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "
            + "where (p.Surname = :p1) and (p = pn.Person) "
            + "order by p.Surname"));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _phoneNumber2, _phoneNumber3 }));
      }
    }

    [Test]
    public void SelectFromFromWhereWhereOrderByOrderBy ()
    {
      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from p in NHQueryFactory.Queryable<Person> (session)
                    from pn in NHQueryFactory.Queryable<PhoneNumber> (session)
                    where p.Surname.Contains ("M")
                    where p == pn.Person
                    orderby pn.AreaCode
                    orderby p.Surname
                    orderby p.FirstName
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p, "
            + "NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "
            + "where (p.Surname like '%'+:p1+'%') and (p = pn.Person) "
            + "order by p.FirstName, p.Surname, pn.AreaCode"));

        var result = query.ToList ();
        Assert.That (result, Is.EqualTo (new[] { _phoneNumber, _phoneNumber4, _phoneNumber5 }));
      }
    }




    [Test]
    public void ComplexTest ()
    {
      var location1 = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 3, "Ouagadougou");
      var location2 = Location.NewObject ("Gassengasse", "12", Country.Australia, 22, "Sydney");
      var location3 = Location.NewObject ("Howard Street", "100", Country.Australia, 22, "Sydney");
      var person1 = Person.NewObject ("Pierre", "Oerson", location2);
      var person2 = Person.NewObject ("Piea", "Muster", location1);
      var person3 = Person.NewObject ("Minny", "Mauser", location2);
      NHibernateSaveOrUpdate (location1, location2, location3, person1, person2, person3);

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from l in NHQueryFactory.Queryable<Location> (session)
                    from p in NHQueryFactory.Queryable<Person> (session)
                    where (((((3 * l.ZipCode - 3) / 7)) == 9) 
                      && p.Surname.Contains ("M") && p.Location == l)  
                    select l;

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] {location2}));
      }
    }


    // Find all Person|s who own their home.
    [Test]
    public void ComplexTest2 ()
    {
      var location1 = Location.NewObject ("Personenstraﬂe", "1111", Country.BurkinaFaso, 3, "Ouagadougou");
      var location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12, "Sydney");
      var location3 = Location.NewObject ("Gassengasse", "22", Country.Austria, 100, "Vienna");
      var person1 = Person.NewObject ("Pierre", "Oerson", location1);
      var person2 = Person.NewObject ("Piea", "Muster", location3);
      var person3 = Person.NewObject ("Minny", "Mauser", location3);

      location1.Owner = person1;
      location2.Owner = person2;
      location3.Owner = person3;

      NHibernateSaveOrUpdate (location1, location2, location3, person1, person2, person3);

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from l in NHQueryFactory.Queryable<Location> (session)
                    from p in NHQueryFactory.Queryable<Person> (session)
                    where (l.Owner == p) && (p.Location == l)
                    select p;

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] {person1, person3}));
      }
    }


    private void SetupTestData ()
    {
      _location = Location.NewObject ("Johnson Street", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
      _location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
      _person = Person.NewObject ("Pierre", "Oerson", _location2);
      _person2 = Person.NewObject ("Max", "Muster", _location);
      _person3 = Person.NewObject ("Minny", "Mauser", _location2);
      _person4 = Person.NewObject ("John", "Johnson", _location2);
      _phoneNumber = PhoneNumber.NewObject ("11111", "2-111", "3-111111", "4-11", _person2);
      _phoneNumber2 = PhoneNumber.NewObject ("22222", "2-222", "3-22222", "4-22", _person);
      _phoneNumber3 = PhoneNumber.NewObject ("11111", "2-333", "3-44444", "4-33", _person);
      _phoneNumber4 = PhoneNumber.NewObject ("11111", "2-444", "3-333333", "4-44444", _person2);
      _phoneNumber5 = PhoneNumber.NewObject ("11111", "2-555", "3-55555", "4-55", _person3);

      NHibernateSaveOrUpdate (_location, _location2, _person, _person2, _person3, _person4);
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

    private IQuery CreateNHQuery (ISession session, Expression queryExpression)
    {
      var queryModel = new QueryParser ().GetParsedQuery (queryExpression);
      return HqlGeneratorQueryModelVisitor.GenerateHqlQuery (queryModel).CreateQuery (session);
    }

    // Takes a queryable and a transformation of that queryable and returns an expression representing that transformation,
    // This is required to get an expression with a result operator such as Count or First.
    // Use as follows:
    // var query = from ... select ...;
    // var countExpression = MakeExpression (query, q => q.Count());
    private Expression MakeExpression<TSource, TResult> (IQueryable<TSource> queryable, Expression<Func<IQueryable<TSource>, TResult>> func)
    {
      return ReplacingExpressionTreeVisitor.Replace (func.Parameters[0], queryable.Expression, func.Body);
    }
  }
}