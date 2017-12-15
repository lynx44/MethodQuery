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

        [Test]
        public void Where_WhenTwoParameters_ReturnsExpectedItem()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser",
                Address = "123 Fake St"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser",
                Address = "456 Fake St"
            });

            var result = this.repository.GetByNameAndAddress("TestUser", "456 Fake St");
            
            Assert.AreEqual(2, result.Id);
        }

        [Test]
        public void Where_WhenParameterIsEnumerable_ReturnsMultipleItems()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser1",
                Address = "123 Fake St"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser2",
                Address = "456 Fake St"
            }, new Person()
            {
                Id = 3,
                Name = "TestUser3",
                Address = "789 Fake St"
            });

            var results = this.repository.Get(new int[] { 1, 3 });

            Assert.AreEqual(2, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 1, 3 }, results.Select(r => r.Id)));
        }

        [Test]
        public void Where_WhenParameterIncludesOrPrefix_UsesOrOperator()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser",
                Address = "123 Fake St"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser2",
                Address = "456 Fake St"
            });

            var results = this.repository.GetByNameOrAddress("TestUser", "456 Fake St");

            Assert.AreEqual(2, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 1, 2 }, results.Select(s => s.Id)));
        }

        [Test]
        public void Where_WhenParameterIncludesMixedOrPrefixes_UsesExpectedOperators()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser2",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 3,
                Name = "TestUser",
                Address = "456 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 4,
                Name = "TestUser3",
                Address = "123 Fake St",
                City = "Portland"
            });

            var results = this.repository.GetByAddressAndCityOrName("123 Fake St", "Seattle", "TestUser");

            Assert.AreEqual(3, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 1, 2, 3 }, results.Select(s => s.Id)));
        }

        [Test]
        public void Where_WhenParameterUsesComplexConditions_UsesExpectedOperators()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser2",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 3,
                Name = "TestUser",
                Address = "456 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 4,
                Name = "TestUser3",
                Address = "123 Fake St",
                City = "Portland"
            });

            var results = this.repository.GetByAddressAndCity(
                new AddressAndCity()
                {
                    Address = "123 Fake St",
                    City = "Seattle"
                });

            Assert.AreEqual(2, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 1, 2 }, results.Select(s => s.Id)));
        }
    }

    public interface IWhereRepository
    {
        IEnumerable<Person> Get(int id);
        Person GetById(int id);
        Person GetByNameAndAddress(string name, string address);
        IEnumerable<Person> Get(IEnumerable<int> id);
        IEnumerable<Person> GetByNameOrAddress(string name, string orAddress);
        IEnumerable<Person> GetByAddressAndCityOrName(string address, string city, string orName);
        IEnumerable<Person> GetByAddressAndCity(AddressAndCity addressAndCity);
    }

    public class AddressAndCity
    {
        public string Address { get; set; }
        public string City { get; set; }
    }

    public class NameAndCity
    {
        public string Name { get; set; }
        public string City { get; set; }
    }
}
