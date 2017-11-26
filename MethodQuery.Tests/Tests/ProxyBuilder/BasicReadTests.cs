using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MethodQuery.Dapper;
using MethodQuery.Tests.Infrastructure.Helpers;
using NUnit.Framework;

namespace MethodQuery.Tests.Tests
{
    [TestFixture]
    public class BasicReadTests
    {
        private MethodQuery.ProxyBuilder proxyBuilder;
        private IRepository repository;
        private DataSeedHelper dataSeedHelper;

        [SetUp]
        public void Setup()
        {
            this.proxyBuilder = new MethodQuery.ProxyBuilder(() => ConnectionFactory.TestDb, new DapperEntityMaterializerFactory());
            this.repository = this.proxyBuilder.Build<IRepository>();
            this.dataSeedHelper = new DataSeedHelper(ConnectionFactory.TestDb);
            this.dataSeedHelper.ClearTable(nameof(Person));
        }

        [Test]
        public void ReadsNone()
        {
            var result = this.repository.GetAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void ReadsOne()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser"
            });
            var result = this.repository.GetAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(1, result.First().Id);
            Assert.AreEqual("TestUser", result.First().Name);
        }

        [Test]
        public void ReadsMultiple()
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
            var result = this.repository.GetAll();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(1, result.First().Id);
            Assert.AreEqual("TestUser1", result.First().Name);
            Assert.AreEqual(2, result.Last().Id);
            Assert.AreEqual("TestUser2", result.Last().Name);
        }
    }

    public interface IRepository
    {
        IEnumerable<Person> GetAll();
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
    }
}
