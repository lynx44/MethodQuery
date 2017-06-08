using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery.Tests.Infrastructure.Extensions
{
    public static class ConnectionExtensions
    {
        public static void EnsureOpen(this IDbConnection connection, Action action)
        {
            var originalState = connection.State;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            action();

            if (originalState == ConnectionState.Closed)
            {
                connection.Close();
            }
        }
    }
}
