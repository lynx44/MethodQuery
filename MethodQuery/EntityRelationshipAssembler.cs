using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery
{
    public class EntityRelationshipAssembler
    {
        public IEnumerable<TEntity> Assemble<TEntity>(EntityCollection entityCollection)
        {
            return entityCollection.Entities.Cast<TEntity>();
        }

        private void AssembleRecursive(EntityCollection entityCollection)
        {
            foreach (var child in entityCollection.Children)
            {
                entityCollection.Entities,
            }
        }
    }

    public class EntityCollection
    {
        public IEnumerable Entities { get; set; }
        public Type EntityType { get; set; }
        public IEnumerable<EntityCollection> Children { get; set; }
    }
}
