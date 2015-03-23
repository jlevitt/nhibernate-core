using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHibernate.Test.NHSpecificTest.Demo
{
	class PersonMapper : ClassMapping<Person>
	{
		public PersonMapper()
		{
			Id(p => p.Id, m => m.Generator(Generators.Identity));
			Table("people");
			Property(p => p.Name);
            Property(p => p.Age);
		}
	}
}