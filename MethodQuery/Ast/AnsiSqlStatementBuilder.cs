using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MethodQuery.Ast
{
    public interface ISqlStatementBuilder
    {
        string BuildStatement(IEnumerable<AstNode> nodes);
    }

    public class AnsiSqlStatementBuilder : ISqlStatementBuilder
    {
        public string BuildStatement(IEnumerable<AstNode> nodes)
        {
            return this.TraverseAst(new StringBuilder(), nodes, false).ToString().Trim();
        }

        private StringBuilder TraverseAst(StringBuilder sb, IEnumerable<AstNode> nodes, bool commaSeparate)
        {
            if (nodes.Any())
            {
                for (var i = 0; i < nodes.Count(); i++)
                {
                    var astNode = nodes.ElementAt(i);
                    sb.Append($"{astNode.QuotedIdentifier}");
                    if (i != nodes.Count() - 1 && commaSeparate)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        sb.Append(" ");
                    }
                    this.TraverseAst(sb, astNode.Args, true);
                }
            }

            return sb;
        }
    }
}
