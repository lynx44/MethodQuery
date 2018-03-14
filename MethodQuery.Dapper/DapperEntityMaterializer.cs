using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace MethodQuery.Dapper
{
    public class DapperEntityMaterializer<TEntity> : IEntityMaterializer<TEntity>
        where TEntity : class
    {
        public IEnumerable<TEntity> Materialize(IDbConnection dbConnection, SqlDirective sqlDirective)
        {
            var sqlParams = new DynamicParameters();
            foreach (var parameter in sqlDirective.Parameters)
            {
                sqlParams.Add(parameter.QuotedIdentifier, parameter.Value);
            }

            return dbConnection.Query<TEntity>(sqlDirective.Sql, (object) sqlParams);
        }
    }

    public class DapperEntityMaterializerFactory : IEntityMaterializerFactory
    {
        public IEntityMaterializer<TEntity> Create<TEntity>()
            where TEntity : class
        {
            return Activator.CreateInstance<DapperEntityMaterializer<TEntity>>();
        }
    }
}
