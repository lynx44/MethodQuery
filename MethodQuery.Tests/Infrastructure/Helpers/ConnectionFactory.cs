using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery.Tests.Infrastructure.Helpers
{
    public static class ConnectionFactory
    {
        public static SqlConnection TestDb => new SqlConnection(ConfigurationManager.ConnectionStrings["MethodQuery.Tests.Properties.Settings.TestDbConnectionString"].ConnectionString);
    }
}
