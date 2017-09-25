using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSearch.Core.Events
{
    /// <summary>
    /// A container for entities that are updated.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityUpdated<T> : ComparableObject<T> where T : BaseEntity
    {

        public EntityUpdated(T entity)
        {
            this.Entity = entity;
        }

        [ObjectSignature]
        public T Entity { get; private set; }
    }
}
