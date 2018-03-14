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
        IEnumerable<TEntity> Materialize(IDbConnection dbConnection, SqlDirective sqlDirective);
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

    public class SqlDirective
    {
        public SqlDirective(string sql, IEnumerable<Parameter> parameters)
        {
            this.Sql = sql;
            this.Parameters = parameters;
        }

        public string Sql { get; set; }
        public IEnumerable<Parameter> Parameters { get; set; }
        public Type OutputType { get; set; }
        public IEnumerable<SqlDirective> Children { get; set; }
    }
}
