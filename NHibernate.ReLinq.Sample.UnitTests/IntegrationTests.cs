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
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Cfg;
using NHibernate.ReLinq.Sample.HqlQueryGeneration;
using NHibernate.ReLinq.Sample.UnitTests.DomainObjects;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.Structure;

namespace NHibernate.ReLinq.Sample.UnitTests
{
	[TestFixture]
	public class IntegrationTests
	{
		#region Fields

		private Configuration _configuration;
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
		private SchemaExport _schemaExport;
		private ISessionFactory _sessionFactory;
		private static readonly string s_currentDirectory = Directory.GetCurrentDirectory();

		#endregion

		#region Properties

		// ReSharper disable PossibleNullReferenceException
		public static DirectoryInfo ProjectDirectory { get; } = new DirectoryInfo(s_currentDirectory).Parent.Parent;
		// ReSharper restore PossibleNullReferenceException

		#endregion

		#region Methods

		[Test]
		public void ComplexTest()
		{
			var location1 = Location.NewObject("Personenstraﬂe", "1111", Country.BurkinaFaso, 3, "Ouagadougou");
			var location2 = Location.NewObject("Gassengasse", "12", Country.Australia, 22, "Sydney");
			var location3 = Location.NewObject("Howard Street", "100", Country.Australia, 22, "Sydney");
			var person1 = Person.NewObject("Pierre", "Oerson", location2);
			var person2 = Person.NewObject("Piea", "Muster", location1);
			var person3 = Person.NewObject("Minny", "Mauser", location2);
			this.NHibernateSaveOrUpdate(location1, location2, location3, person1, person2, person3);

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from l in NHQueryFactory.Queryable<Location>(session)
					from p in NHQueryFactory.Queryable<Person>(session)
					where (((((3 * l.ZipCode - 3) / 7)) == 9)
					       && p.Surname.Contains("M") && p.Location == l)
					select l;

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {location2}));
			}
		}

		// Find all Person|s who own their home.
		[Test]
		public void ComplexTest2()
		{
			var location1 = Location.NewObject("Personenstraﬂe", "1111", Country.BurkinaFaso, 3, "Ouagadougou");
			var location2 = Location.NewObject("Gassengasse", "22", Country.Australia, 12, "Sydney");
			var location3 = Location.NewObject("Gassengasse", "22", Country.Austria, 100, "Vienna");
			var person1 = Person.NewObject("Pierre", "Oerson", location1);
			var person2 = Person.NewObject("Piea", "Muster", location3);
			var person3 = Person.NewObject("Minny", "Mauser", location3);

			location1.Owner = person1;
			location2.Owner = person2;
			location3.Owner = person3;

			this.NHibernateSaveOrUpdate(location1, location2, location3, person1, person2, person3);

			using(var session = this._sessionFactory.OpenSession())
			{
				var query = from l in NHQueryFactory.Queryable<Location>(session)
					from p in NHQueryFactory.Queryable<Person>(session)
					where (l.Owner == p) && (p.Location == l)
					select p;

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {person1, person3}));
			}
		}

		private static void CreateDatabaseIfNecessary(Configuration configuration)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			var connectionString = configuration.Properties["connection.connection_string"];

			var dictionary = connectionString.Split(';').Select(item => item.Split('=')).ToDictionary(itemParts => itemParts[0], itemParts => itemParts[1]);

			const string attachDbFilenameKey = "AttachDbFilename";

			if(dictionary.TryGetValue(attachDbFilenameKey, out var attachDbFilename))
			{
				attachDbFilename = attachDbFilename.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory") + "\\");

				if(!File.Exists(attachDbFilename))
				{
					var fileInformation = new FileInfo(attachDbFilename);

					var databaseName = fileInformation.Name.Substring(0, fileInformation.Name.Length - fileInformation.Extension.Length);

					dictionary.Remove(attachDbFilenameKey);

					var dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");

					using(var dbConnection = dbProviderFactory.CreateConnection())
					{
						// ReSharper disable PossibleNullReferenceException
						dbConnection.ConnectionString = string.Join(";", dictionary.Select(item => item.Key + "=" + item.Value).ToArray());
						// ReSharper restore PossibleNullReferenceException

						dbConnection.Open();
						//var sql = string.Format(CultureInfo.InvariantCulture, "CREATE DATABASE [{0}] ON PRIMARY(NAME= Test_data,

						using(var dbCommand = dbConnection.CreateCommand())
						{
							// ReSharper disable PossibleNullReferenceException
							dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, "CREATE DATABASE [{0}] ON PRIMARY(FILENAME='{1}', NAME='{2}');", databaseName, attachDbFilename, databaseName + "_Data");
							// ReSharper restore PossibleNullReferenceException

							dbCommand.ExecuteNonQuery();
						}
					}
				}
			}
		}

		private IQuery CreateNHQuery(ISession session, Expression queryExpression)
		{
			var queryModel = new QueryParser().GetParsedQuery(queryExpression);
			return HqlGeneratorQueryModelVisitor.GenerateHqlQuery(queryModel).CreateQuery(session);
		}

		// Takes a queryable and a transformation of that queryable and returns an expression representing that transformation,
		// This is required to get an expression with a result operator such as Count or First.
		// Use as follows:
		// var query = from ... select ...;
		// var countExpression = MakeExpression (query, q => q.Count());
		private Expression MakeExpression<TSource, TResult>(IQueryable<TSource> queryable, Expression<Func<IQueryable<TSource>, TResult>> func)
		{
			return ReplacingExpressionTreeVisitor.Replace(func.Parameters[0], queryable.Expression, func.Body);
		}

		[Test]
		public void MyTest()
		{
			// Implement VisitBinaryExpression (And/AndAlso/Or/OrElse)

			using(var session = this._sessionFactory.OpenSession())
			{
				var query = from pn in NHQueryFactory.Queryable<PhoneNumber>(session)
					//where pn.CountryCode == "11111" || pn.Person.FirstName == "Pierre" && pn.Person.Surname == "Oerson"
					where pn.CountryCode.StartsWith("1")
					select pn;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo(
						"select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "
						+ "where ((pn.CountryCode = :p1) or ((pn.Person.FirstName = :p2) and (pn.Person.Surname = :p3)))"));

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {this._phoneNumber, this._phoneNumber2, this._phoneNumber3, this._phoneNumber4, this._phoneNumber5}));
			}
		}

		private void NHibernateSaveOrUpdate(params object[] objectsToSave)
		{
			using(ISession session = this._sessionFactory.OpenSession())
			using(ITransaction transaction = session.BeginTransaction())
			{
				foreach(var o in objectsToSave)
				{
					session.SaveOrUpdate(o);
				}

				transaction.Commit();
			}
		}

		[Test]
		public void SelectFrom()
		{
			// Implement VisitMainFromClause, VisitSelectClause
			// Implement VisitQuerySourceReferenceExpression

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from pn in NHQueryFactory.Queryable<PhoneNumber>(session)
					select pn;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(nhibernateQuery.QueryString, Is.EqualTo("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn"));

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {this._phoneNumber, this._phoneNumber2, this._phoneNumber3, this._phoneNumber4, this._phoneNumber5}));
			}
		}

		[Test]
		public void SelectFromCount()
		{
			// Implement VisitResultOperator

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from pn in NHQueryFactory.Queryable<PhoneNumber>(session)
					select pn;

				var nhibernateQuery = this.CreateNHQuery(session, this.MakeExpression(query, q => q.Count()));
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo("select cast(count(pn) as int) from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn"));

				var result = query.Count();
				Assert.That(result, Is.EqualTo(5));
			}
		}

		[Test]
		public void SelectFromFromWhere()
		{
			// Implement VisitAdditionalFromClause

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from pn in NHQueryFactory.Queryable<PhoneNumber>(session)
					from p in NHQueryFactory.Queryable<Person>(session)
					where pn.Person == p && pn.CountryCode == "22222"
					select p;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo(
						"select p from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn, "
						+ "NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p where ((pn.Person = p) and (pn.CountryCode = :p1))"));

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {this._person}));
			}
		}

		[Test]
		public void SelectFromFromWhereWhereOrderByOrderBy()
		{
			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from p in NHQueryFactory.Queryable<Person>(session)
					from pn in NHQueryFactory.Queryable<PhoneNumber>(session)
					where p.Surname.Contains("M")
					where p == pn.Person
					orderby pn.AreaCode
					orderby p.Surname
					orderby p.FirstName
					select pn;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo(
						"select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p, "
						+ "NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "
						+ "where (p.Surname like '%'+:p1+'%') and (p = pn.Person) "
						+ "order by p.FirstName, p.Surname, pn.AreaCode"));

				var result = query.ToList();
				Assert.That(result, Is.EqualTo(new[] {this._phoneNumber, this._phoneNumber4, this._phoneNumber5}));
			}
		}

		[Test]
		public void SelectFromJoin()
		{
			// Implement VisitJoinClause

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from pn in NHQueryFactory.Queryable<PhoneNumber>(session)
					join p in NHQueryFactory.Queryable<Person>(session) on pn.Person equals p
					where pn.CountryCode == "22222"
					select p;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo(
						"select p from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn, "
						+ "NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p where (pn.Person = p) and (pn.CountryCode = :p1)"));

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {this._person}));
			}
		}

		[Test]
		public void SelectFromOrderBy()
		{
			// Implement VisitOrderByClause

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from pn in NHQueryFactory.Queryable<PhoneNumber>(session)
					orderby pn.Number, pn.CountryCode
					select pn;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn order by pn.Number, pn.CountryCode"));

				var result = query.ToList();
				Assert.That(result, Is.EqualTo(new[] {this._phoneNumber, this._phoneNumber2, this._phoneNumber4, this._phoneNumber3, this._phoneNumber5}));
			}
		}

		[Test]
		public void SelectFromWhere()
		{
			// Implement VisitWhereClause
			// Implement VisitBinaryExpression (Equal), VisitMemberExpression, VisitConstantExpression (+ ParameterAggregator, etc.)

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from pn in NHQueryFactory.Queryable<PhoneNumber>(session)
					where pn.CountryCode == "11111"
					select pn;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo("select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn where (pn.CountryCode = :p1)"));

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {this._phoneNumber, this._phoneNumber3, this._phoneNumber4, this._phoneNumber5}));
			}
		}

		[Test]
		public void SelectFromWhere_WithAndOr()
		{
			// Implement VisitBinaryExpression (And/AndAlso/Or/OrElse)

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from pn in NHQueryFactory.Queryable<PhoneNumber>(session)
					where pn.CountryCode == "11111" || (pn.Person.FirstName == "Pierre" && pn.Person.Surname == "Oerson")
					select pn;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo(
						"select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "
						+ "where ((pn.CountryCode = :p1) or ((pn.Person.FirstName = :p2) and (pn.Person.Surname = :p3)))"));

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {this._phoneNumber, this._phoneNumber2, this._phoneNumber3, this._phoneNumber4, this._phoneNumber5}));
			}
		}

		[Test]
		public void SelectFromWhere_WithContains()
		{
			// Implement VisitMethodCallExpression

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from p in NHQueryFactory.Queryable<Person>(session)
					where p.Surname.Contains(p.FirstName)
					select p;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo("select p from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p where (p.Surname like '%'+p.FirstName+'%')"));

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {this._person4}));
			}
		}

		[Test]
		public void SelectFromWhere_WithPlusMinus()
		{
			// Implement VisitBinaryExpression (Add/Subtract)

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from l in NHQueryFactory.Queryable<Location>(session)
					where ((l.ZipCode + 1) == 12346) || ((l.ZipCode - 99990) == 9)
					select l;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo(
						"select l from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Location as l "
						+ "where (((l.ZipCode + :p1) = :p2) or ((l.ZipCode - :p3) = :p4))"));

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {this._location, this._location2}));
			}
		}

		[Test]
		public void SelectFromWhereOrderByFrom_ClauseOrder()
		{
			// Implicitly sorted via QueryPartsAggregator class

			using(ISession session = this._sessionFactory.OpenSession())
			{
				var query = from p in NHQueryFactory.Queryable<Person>(session)
					where p.Surname == "Oerson"
					orderby p.Surname
					join pn in NHQueryFactory.Queryable<PhoneNumber>(session) on p equals pn.Person
					select pn;

				var nhibernateQuery = this.CreateNHQuery(session, query.Expression);
				Assert.That(
					nhibernateQuery.QueryString,
					Is.EqualTo(
						"select pn from NHibernate.ReLinq.Sample.UnitTests.DomainObjects.Person as p, "
						+ "NHibernate.ReLinq.Sample.UnitTests.DomainObjects.PhoneNumber as pn "
						+ "where (p.Surname = :p1) and (p = pn.Person) "
						+ "order by p.Surname"));

				var result = query.ToList();
				Assert.That(result, Is.EquivalentTo(new[] {this._phoneNumber2, this._phoneNumber3}));
			}
		}

		private static void SetApplicationDataDirectory()
		{
			AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(ProjectDirectory.FullName, "App_Data"));
		}

		[SetUp]
		public void Setup()
		{
			// Create NHibernate DB tables
			this._schemaExport.Execute(false, true, false);

			this.SetupTestData();
		}

		private void SetupTestData()
		{
			this._location = Location.NewObject("Johnson Street", "1111", Country.BurkinaFaso, 99999, "Ouagadougou");
			this._location2 = Location.NewObject("Gassengasse", "22", Country.Australia, 12345, "Sydney");
			this._person = Person.NewObject("Pierre", "Oerson", this._location2);
			this._person2 = Person.NewObject("Max", "Muster", this._location);
			this._person3 = Person.NewObject("Minny", "Mauser", this._location2);
			this._person4 = Person.NewObject("John", "Johnson", this._location2);
			this._phoneNumber = PhoneNumber.NewObject("11111", "2-111", "3-111111", "4-11", this._person2);
			this._phoneNumber2 = PhoneNumber.NewObject("22222", "2-222", "3-22222", "4-22", this._person);
			this._phoneNumber3 = PhoneNumber.NewObject("11111", "2-333", "3-44444", "4-33", this._person);
			this._phoneNumber4 = PhoneNumber.NewObject("11111", "2-444", "3-333333", "4-44444", this._person2);
			this._phoneNumber5 = PhoneNumber.NewObject("11111", "2-555", "3-55555", "4-55", this._person3);

			this.NHibernateSaveOrUpdate(this._location, this._location2, this._person, this._person2, this._person3, this._person4);
		}

		[TearDown]
		public void TearDown()
		{
			// Drop NHibernate DB tables
			this._schemaExport.Drop(false, true);
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			SetApplicationDataDirectory();

			this._configuration = new Configuration();
			this._configuration.Configure();

			CreateDatabaseIfNecessary(this._configuration);

			// Add all NHibernate mapping embedded config resources (i.e. all "*.hbm.xml") from this assembly.
			this._configuration.AddAssembly(this.GetType().Assembly);

			this._sessionFactory = this._configuration.BuildSessionFactory();
			this._schemaExport = new SchemaExport(this._configuration);
		}

		#endregion
	}
}