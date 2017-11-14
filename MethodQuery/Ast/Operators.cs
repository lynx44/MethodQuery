using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery.Ast
{
    public class Operator : AstNode
    {
        public Operator(List<AstNode> args) : base(args)
        {
            
        }
    }

    public class ComparisonOperator : Operator
    {
        public ComparisonOperator(List<AstNode> args) : base(args)
        {
        }
    }

    public class Equals : ComparisonOperator
    {
        public Equals(List<AstNode> args) : base(args)
        {
            this.Identifier = "=";
        }
    }

    public class LogicalOperator : Operator
    {
        public LogicalOperator(List<AstNode> args) : base(args)
        {
        }
    }

    public class AndOperator : LogicalOperator
    {
        public AndOperator(List<AstNode> args) : base(args)
        {
            this.Identifier = "AND";
        }
    }
}
