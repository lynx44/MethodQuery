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
        private MethodIntentParser methodIntentParser;
        private static readonly ParameterDescriptorParser _parameterDescriptorParser = new ParameterDescriptorParser();

        public MethodQueryInterceptor(Func<IDbConnection> connectionFactory, IEntityMaterializerFactory entityMaterializerFactory, IAstFactory astFactory, ISqlStatementBuilder sqlStatementBuilder)
        {
            this.connectionFactory = connectionFactory;
            this.entityMaterializerFactory = entityMaterializerFactory;
            this.sqlStatementBuilder = sqlStatementBuilder;
            this.methodAstBuilder = new MethodAstBuilder(astFactory, _parameterDescriptorParser);
            methodIntentParser = new MethodIntentParser();
        }

        public void Intercept(IInvocation invocation)
        {
            var methodIntent = this.methodIntentParser.GetIntent(invocation.Method);
            var entityType = methodIntent.ReturnType;
            var createMethod = this.entityMaterializerFactory.GetType().GetMethod(nameof(IEntityMaterializerFactory.Create), BindingFlags.Instance | BindingFlags.Public);
            var createEntityMethod = createMethod.MakeGenericMethod(entityType);
            var entityMaterializer = createEntityMethod.Invoke(this.entityMaterializerFactory, new object[0]);
            var materializeMethod = entityMaterializer.GetType().
                GetMethod(nameof(IEntityMaterializer<object>.Materialize), BindingFlags.Instance | BindingFlags.Public);
            var dbConnection = this.connectionFactory();

            var ast = this.methodAstBuilder.BuildAst(methodIntent);
            var sqlStatement = this.sqlStatementBuilder.BuildStatement(ast.Ast);
            var parameters = ast.Parameters.Select(p => new Parameter()
            {
                Identifier = p.AstNamedParameter.Identifier,
                QuotedIdentifier = p.AstNamedParameter.QuotedIdentifier,
                Value = this.GetParameterValue(invocation, p.MethodParameter)
            });
            
            dbConnection.Open();
            var result = materializeMethod.Invoke(entityMaterializer, new object[] { dbConnection, sqlStatement, parameters });
            dbConnection.Close();
            var genericCastMethod = typeof(System.Linq.Enumerable).GetMethod(nameof(System.Linq.Enumerable.Cast), BindingFlags.Public | BindingFlags.Static);
            var castMethod = genericCastMethod.MakeGenericMethod(entityType);

            var enumerableResult = castMethod.Invoke(null, new object[] { result });

            var returnType = invocation.Method.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                invocation.ReturnValue = enumerableResult;
            }
            else
            {
                var genericSingleMethod = typeof(System.Linq.Enumerable).GetMethods().
                    First(m => m.Name == nameof(System.Linq.Enumerable.Single) && m.GetGenericArguments().Length == 1);
                var singleMethod = genericSingleMethod.MakeGenericMethod(entityType);
                invocation.ReturnValue = singleMethod.Invoke(null, new object[] { enumerableResult }); ;
            }
        }

        private object GetParameterValue(IInvocation invocation, ParameterInfo parameter)
        {
            return invocation.GetArgumentValue(
                invocation.Method.GetParameters().
                    Select((p, i) => new Tuple<ParameterInfo, int>(p, i)).
                    First(t => t.Item1.Name == parameter.Name).
                    Item2);
        }
    }

}
