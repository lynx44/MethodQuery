using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MethodQuery.Ast;
using MethodQuery.Extensions;

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

            if (method.Parameters.Length > 0)
            {
                var parameters = this.BuildWhereClause(method.Parameters.Select(p => 
                    new MethodParameter(p.Name, p.ParameterType)
                    {
                        ParameterPath = new ParameterPath()
                        {
                            ParameterInfo = p
                        }
                    }), ast);
                result.Parameters = parameters;
            }

            result.Ast = ast;
            return result;
        }

        private List<MethodAstParamMap> BuildWhereClause(IEnumerable<MethodParameter> parameterList, List<AstNode> ast)
        {
            var parameters = new List<MethodAstParamMap>();
            var conditions = new List<AstNode>();
            var allConditions = conditions;

            this.ParseParameterClause(parameterList, parameters, conditions);

            ast.Add(this.astFactory.Where(allConditions));

            return parameters;
        }

        private void ParseParameterClause(IEnumerable<MethodParameter> parameterList, List<MethodAstParamMap> parameters, List<AstNode> conditions)
        {
            List<AstNode> allConditions;
            if (parameterList.Any(p =>
                        this.parameterDescriptorParser.Describe(p).
                            Attributes.HasFlag(ParameterDescriptorAttributes.OrOperator)))
            {
                var subConditions = new List<AstNode>();
                var andConditions = new List<AstNode>() { this.astFactory.AndOperator(subConditions) };
                conditions.Add(this.astFactory.OrOperator(andConditions));

                allConditions = andConditions;
                conditions = subConditions;
            }
            else
            {
                var subConditions = new List<AstNode>();
                conditions.Add(this.astFactory.AndOperator(subConditions));

                conditions = subConditions;
                allConditions = subConditions;
            }

            for (int i = 0; i < parameterList.Count(); i++)
            {
                var parameterInfo = parameterList.ElementAt(i);

                var parameterDescriptor = this.parameterDescriptorParser.Describe(parameterInfo);
                if (parameterDescriptor.Attributes.HasFlag(ParameterDescriptorAttributes.OrOperator))
                {
                    var subConditions = new List<AstNode>();
                    allConditions.Add(this.astFactory.AndOperator(subConditions));

                    conditions = subConditions;
                }

                if (parameterInfo.ParameterType.IsSqlParameterType())
                {
                    var namedParameter = this.astFactory.NamedParameter(parameterDescriptor.ParameterName);
                    parameters.Add(new MethodAstParamMap()
                    {
                        AstNamedParameter = namedParameter,
                        MethodParameter = parameterInfo
                    });

                    AstNode condition;
                    if (parameterInfo.ParameterType != typeof(string) &&
                        typeof(System.Collections.IEnumerable).IsAssignableFrom(parameterInfo.ParameterType))
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
                    
                    conditions.Add(condition);
                }
                else
                {
                    Type parameterType = parameterInfo.ParameterType;
                    Type actionTypeArg;
                    if (parameterType.TryGetActionType(out actionTypeArg))
                    {
                        parameterType = actionTypeArg;
                    }

                    var classProperties = parameterType.GetProperties().Select(p => new MethodParameter(p.Name, p.PropertyType)
                    {
                        ParameterPath = new ParameterPath(parameterInfo.ParameterPath, p)
                    }).ToList();
                    this.ParseParameterClause(classProperties, parameters, conditions);
                }
            }
        }
    }

    public class ParameterPath
    {
        public ParameterPath()
        {
            this.PropertyPath = new List<PropertyInfo>();
        }

        public ParameterPath(ParameterPath parameter, params PropertyInfo[] additionalPropertyPath)
        {
            this.ParameterInfo = parameter.ParameterInfo;
            this.PropertyPath = parameter.PropertyPath.Concat(additionalPropertyPath).ToList();
        }

        public ParameterInfo ParameterInfo { get; set; }
        public List<PropertyInfo> PropertyPath { get; set; }
    }

    public class MethodParameter
    {
        public MethodParameter(string name, Type parameterType)
        {
            this.Name = name;
            this.ParameterType = parameterType;
        }

        public string Name { get; set; }
        public Type ParameterType { get; set; }
        public ParameterPath ParameterPath { get; set; }

        public string FullName => $"{this.ParameterPath.ParameterInfo.Name}{string.Join("", this.ParameterPath.PropertyPath?.Select(p => p.Name) ?? new List<string>())}";
    }

    public class MethodAstResult
    {
        public MethodAstResult()
        {
            this.Ast = new List<AstNode>();
            this.Parameters = new List<MethodAstParamMap>();
        }

        public IEnumerable<AstNode> Ast { get; set; }
        public IEnumerable<MethodAstParamMap> Parameters { get; set; }
    }

    public class MethodAstParamMap
    {
        public NamedParameter AstNamedParameter { get; set; }
        public MethodParameter MethodParameter { get; set; }
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
        internal ParameterDescriptor Describe(MethodParameter parameter)
        {
            var columnName = parameter.Name;
            var isOrOperator = usesOrOperatorConvention(parameter);
            if (isOrOperator)
            {
                columnName = 
                    parameter.Name
                        .TrimStart("Or".ToCharArray())
                        .TrimStart("or".ToCharArray());
                columnName = char.ToLowerInvariant(columnName[0]) + columnName.Substring(1);
            }

            return new ParameterDescriptor()
            {
                ColumnName = columnName,
                ParameterName = parameter.FullName,
                Attributes = isOrOperator ? ParameterDescriptorAttributes.OrOperator : 0
            };
        }

        private static bool usesOrOperatorConvention(MethodParameter parameter)
        {
            return parameter.Name.StartsWith("or", StringComparison.InvariantCultureIgnoreCase) &&
                   char.IsUpper(parameter.Name.TrimStart("or".ToCharArray())[0]);
        }
    }
}
