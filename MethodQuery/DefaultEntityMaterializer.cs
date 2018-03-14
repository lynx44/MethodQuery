using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery
{
    public class DefaultEntityMaterializer<TEntity> : IEntityMaterializer<TEntity>
    {
        public IEnumerable<TEntity> Materialize(IDbConnection dbConnection, SqlDirective sqlDirective)
        {
            throw new NotImplementedException();
        }
    }
}
