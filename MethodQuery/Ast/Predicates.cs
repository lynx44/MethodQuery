using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery.Ast
{
    public class Predicate : AstNode
    {
        public Predicate(List<AstNode> args) : base(args)
        {
        }
    }

    public class InPredicate : Predicate
    {
        public InPredicate(List<AstNode> args) : base(args)
        {
            this.Identifier = "IN";
        }
    }
}
