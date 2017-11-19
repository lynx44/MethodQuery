using System.Collections.Generic;
using System.Linq;

namespace MethodQuery.Ast
{
    public interface IAstFactory
    {
        SelectStatement Select(IEnumerable<AstNode> args);
        AstNode From(IEnumerable<AstNode> args);
        AstNode Where(IEnumerable<AstNode> args);
        TableIdentifier TableIdentifier(string tableName);
        ColumnIdentifier ColumnIdentifier(string columnName);
        NamedParameter NamedParameter(string parameterName);
        EqualsOperator EqualsOperator(List<AstNode> args);
        AndOperator AndOperator(List<AstNode> args);

        InPredicate InPredicate(List<AstNode> args);
    }

    public class AstFactory : IAstFactory
    {
        // statements
        public SelectStatement Select(IEnumerable<AstNode> args) => new SelectStatement(args.ToList());

        // clauses
        public AstNode From(IEnumerable<AstNode> args) => new FromClause(args.ToList());
        public AstNode Where(IEnumerable<AstNode> args) => new WhereClause(args.ToList());

        // object identifiers
        public TableIdentifier TableIdentifier(string tableName) => new TableIdentifier(tableName);
        public ColumnIdentifier ColumnIdentifier(string columnName) => new ColumnIdentifier(columnName);
        public NamedParameter NamedParameter(string parameterName) => new NamedParameter(parameterName);

        // operators
        public EqualsOperator EqualsOperator(List<AstNode> args) => new EqualsOperator(args);
        public AndOperator AndOperator(List<AstNode> args) => new AndOperator(args);

        // predicates
        public InPredicate InPredicate(List<AstNode> args) => new InPredicate(args);
    }
}