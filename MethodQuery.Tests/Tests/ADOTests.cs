using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MethodQuery.Tests.Infrastructure.Helpers;
using NUnit.Framework;

namespace MethodQuery.Tests.Tests
{
    /// <summary>
    /// Exploratory tests to examine the behavior of default ADO.NET. These tests don't
    /// need to pass
    /// </summary>
    [TestFixture]
    public class ADOTests
    {
        private SqlConnection sqlConnection;
        private DataSeedHelper dataSeedHelper;

        [SetUp]
        public void Setup()
        {
            this.sqlConnection = ConnectionFactory.TestDb;
            this.dataSeedHelper = new DataSeedHelper(sqlConnection);
            this.dataSeedHelper.ClearTable(nameof(Person));
        }

//        [Test]
//        public void InPredicate()
//        {
//            this.dataSeedHelper.SeedTable(new Person()
//            {
//                Id = 1,
//                Name = "TestUser1",
//                Address = "123 Fake St"
//            }, new Person()
//            {
//                Id = 2,
//                Name = "TestUser2",
//                Address = "456 Fake St"
//            }, new Person()
//            {
//                Id = 3,
//                Name = "TestUser3",
//                Address = "789 Fake St"
//            });
//
//            var sqlCommand = new SqlCommand("SELECT Id FROM Person WHERE Id IN @ids", this.sqlConnection);
//            sqlCommand.Parameters.AddWithValue("@ids", new[] { 1, 3 });
//            var adapter = new SqlDataAdapter(sqlCommand);
//            var dataSet = new DataSet();
//            adapter.Fill(dataSet);
//
//            Assert.AreEqual(1, dataSet.Tables.Count);
//            Assert.AreEqual(2, dataSet.Tables[0].Rows);
//        }
    }
}
