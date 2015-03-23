using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.Demo
{
	public class ByCodeFixture : TestCaseMappingByCode
	{
		private int bobId;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.AddMapping<PersonMapper>();

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var bob = new Person
					{
						Age = 45,
						Name = "Bob"
					};
				session.Save(bob);
				tx.Commit();

				bobId = bob.Id;

                session.Clear();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				session.Delete("from System.Object");
				tx.Commit();
			}
		}

		[Test]
		public void Update()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var bob = session.Get<Person>(bobId);
				bob.Age = 3;
				session.Update(bob);
                //tx.Commit();
			}
		}

		[Test]
		public void CachedQueryMissesWithDifferentNotNullComponent()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				using (var dbCommand = session.Connection.CreateCommand())
				{
					dbCommand.CommandText = "DELETE FROM cachedpeople";
					tx.Enlist(dbCommand);
					dbCommand.ExecuteNonQuery();
				}

				tx.Commit();
			}
		}
	}
}