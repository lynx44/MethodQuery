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
}
