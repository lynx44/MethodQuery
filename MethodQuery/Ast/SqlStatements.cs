using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery.Ast
{
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
}
