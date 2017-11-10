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
        IEnumerable<TEntity> Materialize(IDbConnection dbConnection, string sql, IEnumerable<Parameter> parameters);
    }

    public interface IEntityMaterializerFactory
    {
        IEntityMaterializer<TEntity> Create<TEntity>();
    }

    public class Parameter
    {
        public string QuotedIdentifier { get; set; }
        public string Identifier { get; set; }
        public object Value { get; set; }
    }
}
