using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MethodQuery.Tests.Infrastructure.Extensions;

namespace MethodQuery.Tests.Infrastructure.Helpers
{
    public class DataSeedHelper
    {
        private readonly SqlConnection connection;

        public DataSeedHelper(SqlConnection connection)
        {
            this.connection = connection;
        }

        public static void SeedTable(SqlConnection connection, DataTable table)
        {
            connection.EnsureOpen(() =>
            {
                ClearTable(connection, table.TableName);
                var sqlBulkCopy = new SqlBulkCopy(connection);
                sqlBulkCopy.DestinationTableName = table.TableName;
                sqlBulkCopy.WriteToServer(table);
            });
        }

        public static void ClearTable(SqlConnection connection, string tableName)
        {
            connection.EnsureOpen(() =>
            {
                var sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = $"delete from {tableName}";
                sqlCommand.ExecuteNonQuery();
            });
        }

        public void ClearTable(string tableName)
        {
            ClearTable(this.connection, tableName);
        }

        public static void SeedTable<T>(SqlConnection connection, string tableName, params T[] data)
        {
            var dataTable = new DataTable(tableName).AddRows(data);
            SeedTable(connection, dataTable);
        }

        public void SeedTable<T>(string tableName, params T[] data)
        {
            SeedTable(this.connection, tableName, data);
        }

        public void SeedTable<T>(params T[] data)
        {
            this.SeedTable(typeof(T).Name, data);
        }
    }
}
