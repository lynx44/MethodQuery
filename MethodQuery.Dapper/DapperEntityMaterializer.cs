using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace MethodQuery.Dapper
{
    public class DapperEntityMaterializer<TResult> : IEntityMaterializer<TResult>
    {
        public IEnumerable<TResult> Materialize(IDbConnection dbConnection, string sql)
        {
            return dbConnection.Query<TResult>(sql);
        }
    }

    public class DapperEntityMaterializerFactory : IEntityMaterializerFactory
    {
        public IEntityMaterializer<TEntity> Create<TEntity>()
        {
            return Activator.CreateInstance<DapperEntityMaterializer<TEntity>>();
        }
    }
}
