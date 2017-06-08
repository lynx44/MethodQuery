using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery.Tests.Infrastructure.Extensions
{
    public static class DataTableExtensions
    {
        public static DataTable AddRows<T>(this DataTable table, params T[] rowData)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var property in properties)
            {
                var dataColumn = new DataColumn();
                dataColumn.ColumnName = property.Name;
                table.Columns.Add(dataColumn);
            }

            foreach (var row in rowData)
            {
                var dataRow = table.NewRow();
                foreach (var property in properties)
                {
                    dataRow[property.Name] = property.GetValue(row);
                }
                table.Rows.Add(dataRow);
            }

            return table;
        }
    }
}
