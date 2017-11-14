using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MethodQuery.Ast;

namespace MethodQuery
{
    public class MethodAstBuilder
    {
        private readonly IAstFactory astFactory;

        public MethodAstBuilder(IAstFactory astFactory)
        {
            this.astFactory = astFactory;
        }

        public MethodAstResult BuildAst(MethodIntent method)
        {
            var entityType = method.ReturnType;
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var selectStatement = this.astFactory.Select(properties.Select(p => this.astFactory.ColumnIdentifier(p.Name)));
            var fromStatement = this.astFactory.From(new[] { this.astFactory.TableIdentifier(entityType.Name) });

            var ast = new List<AstNode> { selectStatement, fromStatement };

            var result = new MethodAstResult();
            var parameters = new List<MethodAstParamMap>();
            if (method.Parameters.Length > 0)
            {
                var parameterInfo = method.Parameters.First();
                var namedParameter = this.astFactory.NamedParameter(parameterInfo.Name);
                parameters.Add(new MethodAstParamMap()
                {
                    AstNamedParameter = namedParameter,
                    MethodParameter = parameterInfo
                });
                ast.Add(this.astFactory.Where(new List<AstNode> {
                    this.astFactory.EqualsOperator(new List<AstNode>()
                    {
                        this.astFactory.ColumnIdentifier(parameterInfo.Name),
                        namedParameter
                    })
                }
                ));
            }

            result.Ast = ast;
            result.Parameters = parameters;
            return result;
        }
    }

    public class MethodAstResult
    {
        public IEnumerable<AstNode> Ast { get; set; }
        public IEnumerable<MethodAstParamMap> Parameters { get; set; }
    }

    public class MethodAstParamMap
    {
        public NamedParameter AstNamedParameter { get; set; }
        public ParameterInfo MethodParameter { get; set; }
    }
}
