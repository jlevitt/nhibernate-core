using NHibernate.Cfg.MappingSchema;
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
			//Broken at the time NH3634 was reported
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
		public void ShouldBeAbleToQueryAgainstANullComponentProperty()
		{
			//Works at the time NH3634 was reported
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var sally = session.QueryOver<Person>()
								   .Where(p => p.Connection.PortName == null)
								   .SingleOrDefault<Person>();

				Assert.That(sally.Name, Is.EqualTo("Sally"));
				Assert.That(sally.Connection.PortName, Is.Null);
			}
		}
	}
}