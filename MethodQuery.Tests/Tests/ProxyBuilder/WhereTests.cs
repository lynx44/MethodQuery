using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MethodQuery.Dapper;
using MethodQuery.Tests.Infrastructure.Helpers;
using NUnit.Framework.Internal;
using NUnit.Framework;

namespace MethodQuery.Tests.Tests
{
    [TestFixture]
    public class WhereTests
    {
        private MethodQuery.ProxyBuilder proxyBuilder;
        private IWhereRepository repository;
        private DataSeedHelper dataSeedHelper;

        [SetUp]
        public void Setup()
        {
            this.proxyBuilder = new MethodQuery.ProxyBuilder(() => ConnectionFactory.TestDb, new DapperEntityMaterializerFactory());
            this.repository = this.proxyBuilder.Build<IWhereRepository>();
            this.dataSeedHelper = new DataSeedHelper(ConnectionFactory.TestDb);
            this.dataSeedHelper.ClearTable(nameof(Person));
        }

        [Test]
        public void Where_WhenParameterNumeric_ReturnsExpectedEnumerable()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser1"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser2"
            });
            var result = this.repository.Get(2);

            Assert.AreEqual(1, result.Count());
            var entity = result.Single();
            Assert.AreEqual(2, entity.Id);
        }

        [Test]
        public void Where_WhenParameterNumeric_ReturnsExpectedItem()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser1"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser2"
            });
            var result = this.repository.GetById(2);
            
            Assert.AreEqual(2, result.Id);
        }
    }

    public interface IWhereRepository
    {
        IEnumerable<Person> Get(int id);
        Person GetById(int id);
    }
}
