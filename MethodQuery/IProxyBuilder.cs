using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

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
                new MethodQueryInterceptor(this.connectionFactory, this.entityMaterializerFactory)
            );
        }
    }

    internal class MethodQueryInterceptor : IInterceptor
    {
        private readonly Func<IDbConnection> connectionFactory;
        private readonly IEntityMaterializerFactory entityMaterializerFactory;

        public MethodQueryInterceptor(Func<IDbConnection> connectionFactory, IEntityMaterializerFactory entityMaterializerFactory)
        {
            this.connectionFactory = connectionFactory;
            this.entityMaterializerFactory = entityMaterializerFactory;
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
            
            dbConnection.Open();
            var result = materializeMethod.Invoke(entityMaterializer, new object[] { dbConnection, $"SELECT * FROM {entityType.Name}" });
            dbConnection.Close();
            var genericCastMethod = typeof(System.Linq.Enumerable).GetMethod(nameof(System.Linq.Enumerable.Cast), BindingFlags.Public | BindingFlags.Static);
            var castMethod = genericCastMethod.MakeGenericMethod(entityType);



            invocation.ReturnValue = castMethod.Invoke(null, new object[] { result });
        }

        //invocation.Method.ReturnType.GetGenericTypeDefinition().GetInterfaces().Any(i => i == typeof(IEnumerable<>)) ? 
    }
}
