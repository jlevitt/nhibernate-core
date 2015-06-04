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
						Age = 42,
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

        /**
		 * What does this do? Update saves the changes?
		 */
		[Test]
		public void UpdateFlushes()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var bob = session.Get<Person>(bobId);
				bob.Age = 3;
				session.Update(bob);    //<-----
                tx.Commit();            //<-----
			}
		}

        /**
		 * If update doesn't save, what does it do?
		 */
		[Test]
		public void UpdateAttachToSession()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var bob = new Person
					{
						Id = bobId,
						Name = "bill",
                        Age = 13
					};
				session.Update(bob);

				var bob2 = session.Get<Person>(bobId);
				tx.Commit();
			}
		}

		/**
		 * Update with new id. What happens on commit?
		 */
		[Test]
		public void UpdateWithNewId()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var bill = new Person
				{
					Id = 1234,
					Name = "bill",
					Age = 13
				};
				session.Update(bill);

				var bill2 = session.Get<Person>(1234);
				tx.Commit();        //<--------
			}
		}

		/**
		 * Session.Save. When is it flushed? 
		 */
		[Test]
		public void Save()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var stu = new Person
					{
						Name = "stu",
						Age = 17
					};
				session.Save(stu);  //<------
				tx.Commit();        //<------
			}
		}

		/**
		 * FlushMode.Never
		 */
		[Test]
		public void FlushModeNever()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				session.FlushMode = FlushMode.Never;     //<-----

				var bob = session.Get<Person>(bobId);
				bob.Age = 3;
				tx.Commit();
			}
		}

		/**
		 * Session is a cache
		 */
		[Test]
		public void SessionCache()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var bob = session.Get<Person>(bobId);
				bob.Age = 3;

				session.CreateSQLQuery("delete from people")
				       .ExecuteUpdate();

				var bob2 = session.Get<Person>(bobId);      //<-----

                session.Clear();
				var bob3 = session.Get<Person>(bobId);
			}
		}
	}
}