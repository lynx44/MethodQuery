using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MethodQuery.Tests.Tests
{
    [TestFixture]
    public class EntityRelationshipAssemblerTests
    {
        private EntityRelationshipAssembler assembler;

        [SetUp]
        public void Setup()
        {
            this.assembler = new EntityRelationshipAssembler();
        }

        [Test]
        public void Assemble_WhenNoResults_ReturnsEntities()
        {
            var entityCollection = new EntityCollection();
            entityCollection.Entities = new List<Student>();

            var result = this.assembler.Assemble<Student>(entityCollection);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Assemble_WhenNoChildren_ReturnsEntities()
        {
            var entityCollection = new EntityCollection();
            entityCollection.Entities = new List<Student>()
            {
                new Student()
                {
                    Id = 1
                }
            };

            var result = this.assembler.Assemble<Student>(entityCollection);

            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.All(s => !s.Courses.Any()));
        }

        [Test]
        public void Assemble_WhenChildren_ReturnsEntities()
        {
            var entityCollection = new EntityCollection();
            entityCollection.Entities = new List<Student>()
            {
                new Student()
                {
                    Id = 1
                }
            };
            entityCollection.EntityType = typeof(Student);
            entityCollection.Children = new List<EntityCollection>()
            {
                new EntityCollection()
                {
                    Entities = new List<Course>()
                    {
                        new Course()
                        {
                            Id = 2
                        }
                    },
                    EntityType = typeof(Course)
                }
            };

            var result = this.assembler.Assemble<Student>(entityCollection);

            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.All(s => s.Courses.Any()));
        }
    }

    public class Student
    {
        public Student()
        {
            this.Courses = new List<Course>();
        }
        public int Id { get; set; }
        public IEnumerable<Course> Courses { get; set; }
    }

    public class Course
    {
        public int Id { get; set; }
    }
}
