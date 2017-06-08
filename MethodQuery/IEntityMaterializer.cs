using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery
{
    public interface IEntityMaterializer<TEntity>
    {
        IEnumerable<TEntity> Materialize(IDbConnection dbConnection, string sql);
    }

    public interface IEntityMaterializerFactory
    {
        IEntityMaterializer<TEntity> Create<TEntity>();
    }
}
