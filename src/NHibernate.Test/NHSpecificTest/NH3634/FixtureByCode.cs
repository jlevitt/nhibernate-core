using NHibernate.Cfg.MappingSchema;
using NHibernate.Criterion;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3634
{
	public class ByCodeFixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.AddMapping<PersonMapper>();

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var bobsConnection = new Connection
					{
						Address = "test.com",
						ConnectionType = "http",
						PortName = "80"
					};
				var e1 = new Person
					{
						Name = "Bob", 
						Connection = bobsConnection
					};
				session.Save(e1);

				var sallysConnection = new Connection
					{
						Address = "test.com",
						ConnectionType = "http",
					};
				var e2 = new Person
					{
						Name = "Sally", 
						Connection = sallysConnection
					};
				session.Save(e2);

				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");
				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public void ShouldBeAbleToQueryAgainstComponentWithANullProperty()
		{
//			Broken at the time NH3634 was reported
//			Generates the following Rpc(exec sp_executesql)
//			SELECT this_.Id as Id0_0_, 
//				   this_.Name as Name0_0_, 
//				   this_.ConnectionType as Connecti3_0_0_, 
//				   this_.Address as Address0_0_, 
//				   this_.PortName as PortName0_0_ 
//			  FROM people this_ 
//			 WHERE this_.ConnectionType = @p0 
//			   and this_.Address = @p1 
//			   and this_.PortName = @p2
//
//			@p0=N'http',@p1=N'test.com',@p2=NULL

			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var componentToCompare = new Connection
					{
						ConnectionType = "http",
						Address = "test.com", 
						PortName = null
					};
				var sally = session.QueryOver<Person>()
								   .Where(p => p.Connection == componentToCompare)
								   .SingleOrDefault<Person>();

				Assert.That(sally.Name, Is.EqualTo("Sally"));
				Assert.That(sally.Connection.PortName, Is.Null);
			}
		}

		[Test]
		public void ShouldBeAbleToQueryAgainstComponentWithANullPropertyUsingCriteria()
		{
//			Broken at the time NH3634 was reported
//			Generates the following Rpc(exec sp_executesql)
//			SELECT this_.Id as Id0_0_, 
//				   this_.Name as Name0_0_, 
//				   this_.ConnectionType as Connecti3_0_0_, 
//				   this_.Address as Address0_0_, 
//				   this_.PortName as PortName0_0_ 
//			  FROM people this_ 
//			 WHERE this_.ConnectionType = @p0 
//			   and this_.Address = @p1 
//			   and this_.PortName = @p2
//
//			@p0=N'http',@p1=N'test.com',@p2=NULL

			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var componentToCompare = new Connection
				{
					ConnectionType = "http",
					Address = "test.com",
					PortName = null
				};
				var sally = session.CreateCriteria<Person>()
				                   .Add(Restrictions.Eq("Connection", componentToCompare))
				                   .UniqueResult<Person>();

				Assert.That(sally.Name, Is.EqualTo("Sally"));
				Assert.That(sally.Connection.PortName, Is.Null);
			}
		}

		[Test]
		public void ShouldBeAbleToQueryAgainstANullComponentProperty()
		{
//          Works at the time NH3634 was reported 
//			Generates the following SqlBatch:			
//			SELECT this_.Id as Id0_0_, 
//				   this_.Name as Name0_0_, 
//				   this_.ConnectionType as Connecti3_0_0_, 
//				   this_.Address as Address0_0_, 
//				   this_.PortName as PortName0_0_ 
//			  FROM people this_ 
//			 WHERE this_.PortName is null

			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var sally = session.QueryOver<Person>()
				                   .Where(p => p.Connection.PortName == null)
				                   .And(p => p.Connection.Address == "test.com")
				                   .And(p => p.Connection.ConnectionType == "http")
				                   .SingleOrDefault<Person>();

				Assert.That(sally.Name, Is.EqualTo("Sally"));
				Assert.That(sally.Connection.PortName, Is.Null);
			}
		}

		[Test]
		public void ShouldBeAbleToQueryAgainstANullComponentPropertyUsingCriteriaApi()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				NHibernateUtil.HitBreakPoint = true;
				var sally = session.CreateCriteria<Person>()
				                   .Add(Restrictions.Eq("Connection.PortName", null))
				                   .Add(Restrictions.Eq("Connection.Address", "test.com"))
				                   .Add(Restrictions.Eq("Connection.ConnectionType", "http"))
				                   .UniqueResult<Person>();
				NHibernateUtil.HitBreakPoint = false;

				Assert.That(sally.Name, Is.EqualTo("Sally"));
				Assert.That(sally.Connection.PortName, Is.Null);
			}
		}
	}
}