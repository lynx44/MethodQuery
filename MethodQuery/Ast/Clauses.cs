using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery.Ast
{
    public class Clause : AstNode
    {
        public Clause(List<AstNode> args) : base(args)
        {
        }
    }

    public class FromClause : Clause
    {
        public FromClause(List<AstNode> args) : base(args)
        {
            this.Identifier = "FROM";
        }
    }

    public class WhereClause : Clause
    {
        public WhereClause(List<AstNode> args) : base(args)
        {
            this.Identifier = "WHERE";
        }
    }
}
