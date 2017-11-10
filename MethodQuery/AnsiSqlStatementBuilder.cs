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
            return this.TraverseAst(new StringBuilder(), null, nodes).ToString().Trim();
        }

        private StringBuilder TraverseAst(StringBuilder sb, AstNode parentNode, IEnumerable<AstNode> nodes)
        {
            if (nodes.Any())
            {
                for (var i = 0; i < nodes.Count(); i++)
                {
                    var astNode = nodes.ElementAt(i);
                    
                    if ((astNode as ComparisonOperator) == null)
                    {
                        sb.Append($"{astNode.QuotedIdentifier}");
                    }
                    
                    if (i != nodes.Count() - 1 && (parentNode as Statement) != null)
                    {
                        sb.Append(", ");
                    }
                    else if(sb.Length == 0 || sb[sb.Length - 1] != ' ')
                    {
                        sb.Append(" ");
                    }

                    if ((astNode as ComparisonOperator) != null)
                    {
                        this.TraverseAst(sb, astNode, astNode.Args.Take(1));
                        sb.Append($"{astNode.QuotedIdentifier} ");
                        this.TraverseAst(sb, astNode, astNode.Args.Skip(1));
                    }
                    else
                    {
                        this.TraverseAst(sb, astNode, astNode.Args);
                    }
                }
            }

            return sb;
        }
    }
}
