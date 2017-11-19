using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery.Ast
{
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

    public class NamedParameter : AstNode
    {
        private readonly string quotedIdentifier;

        public NamedParameter(string parameterName) : this(parameterName, parameterName)
        {
        }

        public NamedParameter(string parameterName, string columnName)
        {
            this.Identifier = columnName;
            this.quotedIdentifier = $"@{parameterName}";
        }

        public override string QuotedIdentifier => this.quotedIdentifier;
    }
}
