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
    public void SelectFrom ()
    {
      // Implement VisitMainFromClause, VisitSelectClause
      // Implement VisitQuerySourceReferenceExpression

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in new NHQueryable<PhoneNumber> (session)
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString, Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] {_phoneNumber, _phoneNumber2, _phoneNumber3, _phoneNumber4}));
      }
    }

    [Test]
    public void SelectFromWhere ()
    {
      // Implement VisitWhereClause
      // Implement VisitBinaryExpression (Equal), VisitMemberExpression, VisitConstantExpression (+ ParameterAggregator, etc.)

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in new NHQueryable<PhoneNumber> (session)
                    where pn.CountryCode == "11111"
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn where (pn.CountryCode = :p1) "));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _phoneNumber, _phoneNumber3, _phoneNumber4 }));
      }
    }

    [Test]
    public void SelectFromWhere_WithAndOr ()
    {
      // Implement VisitBinaryExpression (And/AndAlso/Or/OrElse)

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in new NHQueryable<PhoneNumber> (session)
                    where pn.CountryCode == "11111" || (pn.Person.FirstName == "Pierre" && pn.Person.Surname == "Oerson")
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString, 
            Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "
                + "where ((pn.CountryCode = :p1) or ((pn.Person.FirstName = :p2) and (pn.Person.Surname = :p3))) "));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _phoneNumber, _phoneNumber2, _phoneNumber3, _phoneNumber4 }));
      }
    }

    [Test]
    public void SelectFromWhere_WithContains ()
    {
      // Implement VisitMethodCallExpression

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from p in new NHQueryable<Person> (session)
                    where p.Surname.Contains (p.FirstName)
                    select p;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select p from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p where p.Surname like '%'+p.FirstName+'%' "));

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
        var query = from pn in new NHQueryable<PhoneNumber> (session)
                    orderby pn.Number, pn.CountryCode
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn order by pn.Number, pn.CountryCode "));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _phoneNumber, _phoneNumber2, _phoneNumber4, _phoneNumber3 }));
      }
    }

    [Test]
    public void SelectFromCount ()
    {
      // Implement VisitResultOperator

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from pn in new NHQueryable<PhoneNumber> (session)
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, MakeExpression (query, q => q.Count()));
        Assert.That (nhibernateQuery.QueryString,
            Is.EqualTo ("select cast(count(*) as int) from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "));

        var result = query.Count();
        Assert.That (result, Is.EqualTo (4));
      }
    }

    [Test]
    [Ignore ("TODO")]
    public void SelectFromWhereFromOrderByWhere ()
    {
      // Implement clause sorting

      using (ISession session = _sessionFactory.OpenSession ())
      {
        var query = from p in new NHQueryable<Person> (session)
                    where p.Surname == "Johnson" || p.Surname.Contains ("M")
                    from pn in p.PhoneNumbers
                    orderby pn.Number
                    where pn.Person == p
                    select pn;

        var nhibernateQuery = CreateNHQuery (session, query.Expression);
        Assert.That (nhibernateQuery.QueryString, Is.EqualTo ("?"));

        var result = query.ToList ();
        Assert.That (result, Is.EquivalentTo (new[] { _person4 }));
      }
    }

    private void SetupTestData ()
    {
      _location = Location.NewObject ("Personenstraße", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
      _location2 = Location.NewObject ("Gassengasse", "22", Country.Australia, 12345, "Sydney");
      _person = Person.NewObject ("Pierre", "Oerson", _location2);
      _person2 = Person.NewObject ("Max", "Muster", _location);
      _person3 = Person.NewObject ("Minny", "Mauser", _location2);
      _person4 = Person.NewObject ("John", "Johnson", _location2);
      _phoneNumber = PhoneNumber.NewObject ("11111", "2-111", "3-111111", "4-11", _person2);
      _phoneNumber2 = PhoneNumber.NewObject ("22222", "2-222", "3-22222", "4-22", _person);
      _phoneNumber3 = PhoneNumber.NewObject ("11111", "2-333", "3-44444", "4-33", _person);
      _phoneNumber4 = PhoneNumber.NewObject ("11111", "2-444", "3-333333", "4-44444", _person2);

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
    // Use like this:
    // var query = from ... select ...;
    // var countExpression = MakeExpression (query, q => q.Count());
    private Expression MakeExpression<TSource, TResult> (IQueryable<TSource> queryable, Expression<Func<IQueryable<TSource>, TResult>> func)
    {
      return ReplacingExpressionTreeVisitor.Replace (func.Parameters[0], queryable.Expression, func.Body);
    }
  }
}