using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace MethodQuery
{
    public class InterfaceEntityMaterializer<TEntity> : IEntityMaterializer<TEntity>
        where TEntity : class
    {
        private ProxyGenerator proxyGenerator = new Castle.DynamicProxy.ProxyGenerator();

        public IEnumerable<TEntity> Materialize(IDbConnection dbConnection, SqlDirective sqlDirective)
        {
            var sqlDataAdapter = new SqlDataAdapter(sqlDirective.Sql, dbConnection as SqlConnection);
            var dataTable = new DataTable();
            sqlDataAdapter.Fill(dataTable);
            var columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

            return dataTable.Rows.Cast<DataRow>().
                Select(dr => this.proxyGenerator.CreateInterfaceProxyWithoutTarget<TEntity>(
                    new EntityProxyInterceptor(ToDictionary(columnNames, dr))))
                    .ToList();
        }

        private IDictionary<string, object> ToDictionary(string[] columnNames, DataRow dataRow)
        {
            var dictionary = new ConcurrentDictionary<string, object>();
            for (var index = 0; index < columnNames.Length; index++)
            {
                var columnName = columnNames[index];
                dictionary[columnName] = dataRow.ItemArray[index];
            }

            return dictionary;
        }

        private class EntityProxyInterceptor : IInterceptor
        {
            private readonly IDictionary<string, object> properties;

            public EntityProxyInterceptor(IDictionary<string, object> properties)
            {
                this.properties = properties;
            }

            public void Intercept(IInvocation invocation)
            {
                var propertyInfo = invocation.Method.DeclaringType.GetProperties()
                    .FirstOrDefault(prop => prop.GetGetMethod() == invocation.Method);
                if (propertyInfo != null)
                {
                    var keyName = this.properties.Keys.FirstOrDefault(
                        k => k.Equals(propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase));

                    if (keyName != null)
                    {
                        invocation.ReturnValue = this.properties[keyName];
                    }
                }
            }
        }
    }
}
