using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery.Ast
{
    public class MethodAstBuilder
    {
        private readonly IAstFactory astFactory;

        public MethodAstBuilder(IAstFactory astFactory)
        {
            this.astFactory = astFactory;
        }

        public IEnumerable<AstNode> BuildAst(MethodInfo method)
        {
            var entityType = method.ReturnType.GetGenericArguments().First();
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var selectStatement = this.astFactory.Select(properties.Select(p => this.astFactory.ColumnIdentifier(p.Name)));
            var fromStatement = this.astFactory.From(new[] { this.astFactory.TableIdentifier(entityType.Name) });

            return new[] { selectStatement, fromStatement };
        }
    }
}
