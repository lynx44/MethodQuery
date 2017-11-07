using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MethodQuery.Ast;
using NUnit.Framework;

namespace MethodQuery.Tests.Tests.Ast
{
    [TestFixture]
    public class AstTests
    {
        private AstFactory astFactory;

        [SetUp]
        public void Setup()
        {
            this.astFactory = new AstFactory();
        }

        [Test]
        public void BasicSelect()
        {
            var ast = new AstNode[]
            {
                this.astFactory.Select(new List<AstNode>()
                {
                    this.astFactory.ColumnIdentifier("Id")
                }),
                this.astFactory.From(new List<AstNode>()
                {
                    this.astFactory.TableIdentifier("Person")
                })
            };

            var builder = new AnsiSqlStatementBuilder();
            var statement = builder.BuildStatement(ast);

            Assert.AreEqual("SELECT \"Id\" FROM \"Person\"", statement);
        }

        [Test]
        public void SelectMultipleExpressions()
        {
            var ast = new AstNode[]
            {
                this.astFactory.Select(new List<AstNode>()
                {
                    this.astFactory.ColumnIdentifier("Id"),
                    this.astFactory.ColumnIdentifier("Name")
                }),
                this.astFactory.From(new List<AstNode>()
                {
                    this.astFactory.TableIdentifier("Person")
                })
            };

            var builder = new AnsiSqlStatementBuilder();
            var statement = builder.BuildStatement(ast);

            Assert.AreEqual("SELECT \"Id\", \"Name\" FROM \"Person\"", statement);
        }
    }
}
