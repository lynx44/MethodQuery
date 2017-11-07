using System.Collections.Generic;
using System.Linq;

namespace MethodQuery.Ast
{
    public interface IAstFactory
    {
        SelectStatement Select(IEnumerable<AstNode> args);
        ColumnIdentifier ColumnIdentifier(string columnName);
        AstNode From(IEnumerable<AstNode> args);
        TableIdentifier TableIdentifier(string tableName);
    }

    public class AstFactory : IAstFactory
    {
        public SelectStatement Select(IEnumerable<AstNode> args) => new SelectStatement(args.ToList());
        public ColumnIdentifier ColumnIdentifier(string columnName) => new ColumnIdentifier(columnName);
        public AstNode From(IEnumerable<AstNode> args) => new FromClause(args.ToList());
        public TableIdentifier TableIdentifier(string tableName) => new TableIdentifier(tableName);
    }
}