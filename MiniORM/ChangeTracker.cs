using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MiniORM
{
    internal class ChangeTracker<T>
        where T : class, new()
    {
        private readonly List<T> allEntitites;
        private readonly List<T> added;
        private readonly List<T> removed;

        public ChangeTracker(IEnumerable<T> entities)
        {
            added = new List<T>();
            removed = new List<T>();

            allEntitites = CloneEntities(entities);
        }

        public IReadOnlyCollection<T> Added => this.added.AsReadOnly();
        public IReadOnlyCollection<T> AllEntities => this.allEntitites.AsReadOnly();
        public IReadOnlyCollection<T> Removed => this.removed.AsReadOnly();

        public void Add(T item) => this.added.Add(item);
        public void Remove(T item) => removed.Add(item);
        public List<T> CloneEntities(IEnumerable<T> entities)
        {
            var clonedEntities = new List<T>();

            var propertiesToClone = typeof(T).GetProperties()
                                   .Where(p => DbContext.AllowedSqlTypes.Contains(p.PropertyType))
                                   .ToArray();

            foreach (var entity in entities)
            {
                var clonedEntity = Activator.CreateInstance<T>();

                foreach (var property in propertiesToClone)
                {
                    var value = property.GetValue(entity);
                    property.SetValue(clonedEntity, value);
                }

                clonedEntities.Add(entity);
            }


            return clonedEntities;
        }
        public IEnumerable<T> GetModifiedEntities(DbSet<T> set)
        {
            var modifiedEntities = new List<T>();

            var primaryKeys = typeof(T)
                .GetProperties()
                .Where(pk => pk.HasAttribute<KeyAttribute>())
                .ToArray();

            foreach (var proxy in this.AllEntities)
            {
                var primaryKeyValues = GetPrimaryKeyValues(primaryKeys, proxy).ToArray();

                var entity = set.Entities
                    .Single(s => GetPrimaryKeyValues(primaryKeys, s).SequenceEqual(primaryKeyValues));
                var isModified = IsModified(proxy, entity);

                if (isModified)
                {
                    modifiedEntities.Add(entity);
                }
            }

            return modifiedEntities;
        }

        private static bool IsModified(T proxy, T entity)
        {
            var monitoredProperties = typeof(T)
                .GetProperties()
                .Where(p => DbContext.AllowedSqlTypes.Contains(p.PropertyType));

            var modifiedProperties = typeof(T)
                .GetProperties()
                .Where(p => !Equals(p.GetValue(entity), p.GetValue(proxy)))
                .ToArray();

            return modifiedProperties.Count() > 0;
        }

        private IEnumerable<object> GetPrimaryKeyValues(PropertyInfo[] primaryKeys, T entity)
        {
            return primaryKeys.Select(pk => pk.GetValue(entity));
        }
    }
}