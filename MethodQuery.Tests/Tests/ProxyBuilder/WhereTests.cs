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
        public void Where_WhenBasicOrBasicParameters_UsesOrOperator()
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
        public void Where_WhenBasicAndBasicOrBasicParameters_UsesExpectedOperators()
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
        public void Where_WhenComplexAndParameter_UsesExpectedOperators()
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

        [Test]
        public void Where_WhenComplexOrAndBasicParameters_UsesExpectedOperators()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser1",
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
                Name = "TestUser",
                Address = "123 Fake St",
                City = "Portland"
            });

            var results = this.repository.GetByAddressOrCityAndName(
                new AddressOrCity()
                {
                    Address = "123 Fake St",
                    OrCity = "Seattle"
                },
                "TestUser");

            Assert.AreEqual(2, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 3, 4 }, results.Select(s => s.Id)));
        }

        [Test]
        public void Where_WhenComplexAndOrComplexAndParameters_UsesExpectedOperators()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser1",
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
                Name = "TestUser",
                Address = "123 Fake St",
                City = "Portland"
            });

            var results = this.repository.GetByAddressAndCityOrNameAndCity(
                new AddressAndCity()
                {
                    Address = "123 Fake St",
                    City = "Seattle"
                },
                new NameAndCity()
                {
                    Name = "TestUser",
                    City = "Seattle"
                });

            Assert.AreEqual(3, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 1, 2, 3 }, results.Select(s => s.Id)));
        }

        [Test]
        public void Where_WhenComplexAndOrComplexAndOrComplexOrParameters_UsesExpectedOperators()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser1",
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
                Name = "TestUser",
                Address = "123 Fake St",
                City = "Portland"
            }, new Person()
            {
                Id = 5,
                Name = "TestUser4",
                Address = "456 Fake St",
                City = "Portland"
            }, new Person()
            {
                Id = 6,
                Name = "TestUser4",
                Address = "456 Fake St",
                City = "Santa Fe"
            });

            var results = this.repository.GetByAddressAndCityOrNameAndCityOrAddressOrCity(
                new AddressAndCity()
                {
                    Address = "123 Fake St",
                    City = "Seattle"
                },
                new NameAndCity()
                {
                    Name = "TestUser",
                    City = "Seattle"
                },
                new AddressOrCity()
                {
                    Address = "123 Fake St",
                    OrCity = "Portland"
                });

            Assert.AreEqual(5, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 1, 2, 3, 4, 5 }, results.Select(s => s.Id)));
        }

        [Test]
        public void Where_WhenComplexOrNestedComplexAndParameter_UsesExpectedOperators()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser2",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser",
                Address = "456 Fake St",
                City = "Santa Fe"
            }, new Person()
            {
                Id = 3,
                Name = "TestUser3",
                Address = "456 Fake St",
                City = "Portland"
            });

            var results = this.repository.GetByNameOrAddressAndCity(
                new NameOrAddressAndCity()
                {
                    Name = "TestUser",
                    OrAddressAndCity =
                        new AddressAndCity()
                        {
                            Address = "123 Fake St",
                            City = "Seattle"
                        }
                });

            Assert.AreEqual(2, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 1, 2 }, results.Select(s => s.Id)));
        }

        [Test]
        public void Where_WhenComplexAndNestedComplexOrAndParameter_UsesExpectedOperators()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser2",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 3,
                Name = "TestUser3",
                Address = "123 Fake St",
                City = "Seattle"
            });

            var results = this.repository.GetByIdOrNameAndAddressAndCity(
                new IdOrNameAndAddressAndCity()
                {
                    IdOrName = new IdOrName()
                    {
                        Id = 1,
                        OrName = "TestUser"
                    },
                    AddressAndCity =
                        new AddressAndCity()
                        {
                            Address = "123 Fake St",
                            City = "Seattle"
                        }
                });

            Assert.AreEqual(2, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 1, 2 }, results.Select(s => s.Id)));
        }

        [Test]
        public void Where_WhenBasicAndNestedComplexOrNestedComplexAndParameter_UsesExpectedOperators()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser2",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 3,
                Name = "TestUser3",
                Address = "123 Fake St",
                City = "Seattle"
            });

            var results = this.repository.GetByIdAndNameOrAddressAndCity(
                new IdAndNameOrAddressAndCity()
                {
                    Id = 1,
                    NameOrAddressAndCity = 
                        new NameOrAddressAndCity()
                        {
                            Name = "TestUser",
                            OrAddressAndCity = new AddressAndCity()
                            {
                                Address = "123 Fake St",
                                City = "Seattle"
                            }
                        }
                });

            Assert.AreEqual(1, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 1 }, results.Select(s => s.Id)));
        }

        [Test]
        public void Where_WhenBasicAndNestedComplexOrNestedComplexAndParameter_Scenario2_UsesExpectedOperators()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser2",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 3,
                Name = "TestUser3",
                Address = "123 Fake St",
                City = "Seattle"
            });

            var results = this.repository.GetByIdAndNameOrAddressAndCity(
                new IdAndNameOrAddressAndCity()
                {
                    Id = 1,
                    NameOrAddressAndCity = 
                        new NameOrAddressAndCity()
                        {
                            Name = "TestUser2",
                            OrAddressAndCity = new AddressAndCity()
                            {
                                Address = "124 Fake St",
                                City = "Seattle"
                            }
                        }
                });

            Assert.AreEqual(1, results.Count());
            Assert.IsTrue(Enumerable.SequenceEqual(new [] { 1 }, results.Select(s => s.Id)));
        }

        [Test]
        public void Where_WhenBasicAndNestedComplexOrNestedComplexAndParameter_ReturnsNone()
        {
            this.dataSeedHelper.SeedTable(new Person()
            {
                Id = 1,
                Name = "TestUser2",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 2,
                Name = "TestUser",
                Address = "123 Fake St",
                City = "Seattle"
            }, new Person()
            {
                Id = 3,
                Name = "TestUser3",
                Address = "123 Fake St",
                City = "Seattle"
            });

            var results = this.repository.GetByIdAndNameOrAddressAndCity(
                new IdAndNameOrAddressAndCity()
                {
                    Id = 1,
                    NameOrAddressAndCity = 
                        new NameOrAddressAndCity()
                        {
                            Name = "TestUser3",
                            OrAddressAndCity = new AddressAndCity()
                            {
                                Address = "124 Fake St",
                                City = "Seattle"
                            }
                        }
                });

            Assert.AreEqual(0, results.Count());
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
        IEnumerable<Person> GetByAddressOrCityAndName(AddressOrCity addressAndCity, string name);
        IEnumerable<Person> GetByAddressAndCityOrNameAndCity(AddressAndCity addressAndCity, NameAndCity orNameAndCity);
        IEnumerable<Person> GetByAddressAndCityOrNameAndCityOrAddressOrCity(AddressAndCity addressAndCity, NameAndCity orNameAndCity, AddressOrCity orAddressOrCity);
        IEnumerable<Person> GetByNameOrAddressAndCity(NameOrAddressAndCity nameOrAddressAndCity);
        IEnumerable<Person> GetByIdOrNameAndAddressAndCity(IdOrNameAndAddressAndCity idOrNameAndAddressAndCity);
        IEnumerable<Person> GetByIdAndNameOrAddressAndCity(IdAndNameOrAddressAndCity idAndNameOrAddressAndCity);
    }

    public class AddressAndCity
    {
        public string Address { get; set; }
        public string City { get; set; }
    }

    public class AddressOrCity
    {
        public string Address { get; set; }
        public string OrCity { get; set; }
    }

    public class NameAndCity
    {
        public string Name { get; set; }
        public string City { get; set; }
    }

    public class NameOrAddressAndCity
    {
        public string Name { get; set; }
        public AddressAndCity OrAddressAndCity { get; set; }
    }

    public class IdOrName
    {
        public int Id { get; set; }
        public string OrName { get; set; }
    }

    public class IdOrNameAndAddressAndCity
    {
        public IdOrName IdOrName { get; set; }
        public AddressAndCity AddressAndCity { get; set; }
    }

    public class IdAndNameOrAddressAndCity
    {
        public int Id { get; set; }
        public NameOrAddressAndCity NameOrAddressAndCity { get; set; }
    }
}
