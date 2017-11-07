using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using MethodQuery.Ast;

namespace MethodQuery
{
    public interface IProxyBuilder
    {
        TProxy Build<TProxy>() where TProxy : class;
    }

    public class ProxyBuilder : IProxyBuilder
    {
        private readonly Func<IDbConnection> connectionFactory;
        private readonly IEntityMaterializerFactory entityMaterializerFactory;

        public ProxyBuilder(Func<IDbConnection> connectionFactory, IEntityMaterializerFactory entityMaterializerFactory)
        {
            this.connectionFactory = connectionFactory;
            this.entityMaterializerFactory = entityMaterializerFactory;
        }

        public TProxy Build<TProxy>() where TProxy : class
        {
            return new Castle.DynamicProxy.ProxyGenerator().CreateInterfaceProxyWithoutTarget<TProxy>(
                new MethodQueryInterceptor(this.connectionFactory, this.entityMaterializerFactory, new AstFactory(), new AnsiSqlStatementBuilder())
            );
        }
    }

    internal class MethodQueryInterceptor : IInterceptor
    {
        private readonly Func<IDbConnection> connectionFactory;
        private readonly IEntityMaterializerFactory entityMaterializerFactory;
        private readonly ISqlStatementBuilder sqlStatementBuilder;
        private MethodAstBuilder methodAstBuilder;

        public MethodQueryInterceptor(Func<IDbConnection> connectionFactory, IEntityMaterializerFactory entityMaterializerFactory, IAstFactory astFactory, ISqlStatementBuilder sqlStatementBuilder)
        {
            this.connectionFactory = connectionFactory;
            this.entityMaterializerFactory = entityMaterializerFactory;
            this.sqlStatementBuilder = sqlStatementBuilder;
            this.methodAstBuilder = new MethodAstBuilder(astFactory);
        }

        public void Intercept(IInvocation invocation)
        {
            var entityType = invocation.Method.ReturnType.GetGenericArguments().First();
            var createMethod = this.entityMaterializerFactory.GetType().GetMethod(nameof(IEntityMaterializerFactory.Create), BindingFlags.Instance | BindingFlags.Public);
            var createEntityMethod = createMethod.MakeGenericMethod(entityType);
            var entityMaterializer = createEntityMethod.Invoke(this.entityMaterializerFactory, new object[0]);
            var materializeMethod = entityMaterializer.GetType().
                GetMethod(nameof(IEntityMaterializer<object>.Materialize), BindingFlags.Instance | BindingFlags.Public);
            var dbConnection = this.connectionFactory();

            var ast = this.methodAstBuilder.BuildAst(invocation.Method);
            var sqlStatement = this.sqlStatementBuilder.BuildStatement(ast);
            
            dbConnection.Open();
            var result = materializeMethod.Invoke(entityMaterializer, new object[] { dbConnection, sqlStatement });
            dbConnection.Close();
            var genericCastMethod = typeof(System.Linq.Enumerable).GetMethod(nameof(System.Linq.Enumerable.Cast), BindingFlags.Public | BindingFlags.Static);
            var castMethod = genericCastMethod.MakeGenericMethod(entityType);



            invocation.ReturnValue = castMethod.Invoke(null, new object[] { result });
        }

        //invocation.Method.ReturnType.GetGenericTypeDefinition().GetInterfaces().Any(i => i == typeof(IEnumerable<>)) ? 
    }
}
