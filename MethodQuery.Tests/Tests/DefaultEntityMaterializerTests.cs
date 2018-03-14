using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MethodQuery.Tests.Infrastructure.Helpers;
using NUnit.Framework;

namespace MethodQuery.Tests.Tests
{
    [TestFixture]
    public class DefaultEntityMaterializerTests
    {
        private DefaultEntityMaterializer<IPerson> materializer;
        private DataSeedHelper dataSeedHelper;

        [SetUp]
        public void Setup()
        {
            this.materializer = new DefaultEntityMaterializer<IPerson>();
            this.dataSeedHelper = new DataSeedHelper(ConnectionFactory.TestDb);
            this.dataSeedHelper.ClearTable(nameof(Person));
        }

        [Test]
        public void MaterializesSingleObject()
        {
            this.dataSeedHelper.SeedTable(new Person() { Id = 1 });
            var entities = this.materializer.Materialize(ConnectionFactory.TestDb, new SqlDirective("SELECT Id FROM Person", typeof(IPerson)));

            Assert.AreEqual(1, entities.Count());
        }

        [Test]
        public void MaterializesInteger()
        {
            this.dataSeedHelper.SeedTable(new Person() { Id = 5 });

            var entities = this.materializer.Materialize(ConnectionFactory.TestDb, new SqlDirective("SELECT Id FROM Person", typeof(IPerson)));

            var entity = entities.First();
            Assert.AreEqual(5, entity.Id);
        }
    }

    public interface IPerson
    {
        int Id { get; }
    }
}
