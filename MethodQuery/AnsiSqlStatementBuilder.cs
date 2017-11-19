using System.Collections.Generic;
using System.Linq;
using System.Text;
using MethodQuery.Ast;

namespace MethodQuery
{
    public interface ISqlStatementBuilder
    {
        string BuildStatement(IEnumerable<AstNode> nodes);
    }

    public class AnsiSqlStatementBuilder : ISqlStatementBuilder
    {
        public string BuildStatement(IEnumerable<AstNode> nodes)
        {
            return this.TraverseAst(new StringBuilder(), null, nodes, false).ToString().Trim();
        }

        private StringBuilder TraverseAst(StringBuilder sb, AstNode parentNode, IEnumerable<AstNode> nodes, bool isListLogicallyGrouped)
        {
            if (nodes.Any())
            {
                for (var i = 0; i < nodes.Count(); i++)
                {
                    var astNode = nodes.ElementAt(i);

                    var isOperator = (astNode as Operator) != null;
                    var isPredicate = (astNode as Predicate) != null;
                    if (!isOperator && !isPredicate)
                    {
                        sb.Append($"{astNode.QuotedIdentifier}");
                    }
                    
                    if (i != nodes.Count() - 1 && ((parentNode as Statement) != null || (parentNode as Predicate) != null))
                    {
                        sb.Append(", ");
                    }
                    else if((sb.Length == 0 || sb[sb.Length - 1] != ' ') && (!isListLogicallyGrouped || i != nodes.Count() - 1))
                    {
                        sb.Append(" ");
                    }

                    if (isOperator || isPredicate)
                    {
                        this.TraverseAst(sb, astNode, astNode.Args.Take(1), false);
                        if (!isOperator || (isOperator && astNode.Args.Skip(1).Any()))
                        {
                            sb.Append($"{astNode.QuotedIdentifier} ");

                            this.TraverseAst(sb, astNode, astNode.Args.Skip(1), false);
                        }
                    }
                    else
                    {
                        this.TraverseAst(sb, astNode, astNode.Args, false);
                    }
                }
            }

            return sb;
        }
    }
}
