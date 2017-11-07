using System.Collections.Generic;

namespace MethodQuery.Ast
{
    public abstract class AstNode
    {
        protected AstNode() : this(new List<AstNode>())
        {
        }

        protected AstNode(List<AstNode> args)
        {
            this.Args = args;
        }

        public string Identifier { get; set; }
        public virtual string QuotedIdentifier => this.Identifier;

        public List<AstNode> Args { get; }
    }

    public class ObjectIdentifier : AstNode
    {
        public override string QuotedIdentifier => $"\"{this.Identifier}\"";
    }

    public class ColumnIdentifier : ObjectIdentifier
    {
        public ColumnIdentifier(string columnName)
        {
            this.Identifier = columnName;
        }
    }

    public class TableIdentifier : ObjectIdentifier
    {
        public TableIdentifier(string tableName)
        {
            this.Identifier = tableName;
        }
    }

    public class Statement : AstNode
    {
        public Statement(List<AstNode> args) : base(args)
        {
            
        }
        public AstNode Clause { get; set; }
    }

    public class SelectStatement : Statement
    {
        public SelectStatement(List<AstNode> args) : base(args)
        {
            this.Identifier = "SELECT";
        }
    }

    public class FromClause : AstNode
    {
        public FromClause(List<AstNode> args) : base(args)
        {
            this.Identifier = "FROM";
        }
    }

    public class Property : AstNode
    {
    }
}
