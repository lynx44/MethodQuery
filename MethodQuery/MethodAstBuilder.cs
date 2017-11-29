using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MethodQuery.Ast;

namespace MethodQuery
{
    public class MethodAstBuilder
    {
        private readonly IAstFactory astFactory;
        private readonly ParameterDescriptorParser parameterDescriptorParser;

        public MethodAstBuilder(IAstFactory astFactory, ParameterDescriptorParser parameterDescriptorParser)
        {
            this.astFactory = astFactory;
            this.parameterDescriptorParser = parameterDescriptorParser;
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

            if(method.Parameters.Length > 0) { 
                var conditions = new List<AstNode>();
                var allConditions = conditions;
                
                Func<List<AstNode>, AstNode> operatorMethod = null;
                for (int i = 0; i < method.Parameters.Length; i++)
                {
                    var parameterInfo = method.Parameters.ElementAt(i);
                    var parameterDescriptor = this.parameterDescriptorParser.Describe(parameterInfo);
                    var namedParameter = this.astFactory.NamedParameter(parameterDescriptor.ParameterName);
                    parameters.Add(new MethodAstParamMap()
                    {
                        AstNamedParameter = namedParameter,
                        MethodParameter = parameterInfo
                    });

                    AstNode condition;
                    if (parameterInfo.ParameterType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(parameterInfo.ParameterType))
                    {
                        condition = this.astFactory.InPredicate(new List<AstNode>()
                        {
                            this.astFactory.ColumnIdentifier(parameterDescriptor.ColumnName),
                            namedParameter
                        });
                    }
                    else
                    {
                        condition = this.astFactory.EqualsOperator(new List<AstNode>()
                        {
                            this.astFactory.ColumnIdentifier(parameterDescriptor.ColumnName),
                            namedParameter
                        });
                    }

                    var previousOperatorMethod = operatorMethod;
                    if (parameterDescriptor.Attributes.HasFlag(ParameterDescriptorAttributes.OrOperator))
                    {
                        operatorMethod = this.astFactory.OrOperator;
                    }
                    else
                    {
                        operatorMethod = this.astFactory.AndOperator;
                    }

                    if (previousOperatorMethod != operatorMethod)
                    {
                        var subConditions = new List<AstNode>();
                        conditions.Add(operatorMethod(subConditions));

                        conditions = subConditions;
                    }

                    conditions.Add(condition);
                }

                ast.Add(this.astFactory.Where(allConditions));

                //                if (parameters.Count > 1)
                //                {
                //                    ast.Add(this.astFactory.Where(new List<AstNode>() { operatorMethod(conditions) }));
                //                }
                //                else
                //                {
                //                    ast.Add(this.astFactory.Where(conditions));
                //                }
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
        public ParameterDescriptor ParameterDescriptor { get; set; }
    }

    public class ParameterDescriptor
    {
        public string ColumnName { get; set; }
        public string ParameterName { get; set; }
        public ParameterDescriptorAttributes Attributes { get; set; }
    }

    [Flags]
    public enum ParameterDescriptorAttributes
    {
        None,
        OrOperator
    }

    public class ParameterDescriptorParser
    {
        public ParameterDescriptor Describe(ParameterInfo parameter)
        {
            var columnName = parameter.Name;
            var isOrOperator = usesOrOperatorConvention(parameter);
            if (isOrOperator)
            {
                columnName = 
                    parameter.Name
                        .TrimStart("or".ToCharArray());
                columnName = char.ToLowerInvariant(columnName[0]) + columnName.Substring(1);
            }

            return new ParameterDescriptor()
            {
                ColumnName = columnName,
                ParameterName = columnName,
                Attributes = isOrOperator ? ParameterDescriptorAttributes.OrOperator : 0
            };
        }

        private static bool usesOrOperatorConvention(ParameterInfo parameter)
        {
            return parameter.Name.StartsWith("or", StringComparison.InvariantCultureIgnoreCase) &&
                   char.IsUpper(parameter.Name.TrimStart("or".ToCharArray())[0]);
        }
    }
}
