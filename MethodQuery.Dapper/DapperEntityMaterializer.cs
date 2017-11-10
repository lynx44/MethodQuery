﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace MethodQuery.Dapper
{
    public class DapperEntityMaterializer<TResult> : IEntityMaterializer<TResult>
    {
        public IEnumerable<TResult> Materialize(IDbConnection dbConnection, string sql, IEnumerable<Parameter> parameters)
        {
            var sqlParams = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                sqlParams.Add(parameter.QuotedIdentifier, parameter.Value);
            }

            return dbConnection.Query<TResult>(sql, (object) sqlParams);
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
