using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Orchard.Data;

namespace Microsoft.CloudMedia.Tests.Common {
    public class StubRepository<T> : IRepository<T> {
        private int _seed;
        private readonly IDictionary<int, T> _dictionary = new Dictionary<int, T>();

        public void Create(T entity) {
            SetId(entity, ++_seed);
            _dictionary[_seed] = entity;
        }

        public void Update(T entity) { }

        public void Delete(T entity) {
            _dictionary.Remove(GetId(entity));
        }

        public void Copy(T source, T target) {}
        public void Flush() {}

        public T Get(int id) {
            return _dictionary.ContainsKey(id) ? _dictionary[id] : default(T);
        }

        public T Get(Expression<Func<T, bool>> predicate) {
            return _dictionary.Values.SingleOrDefault(predicate.Compile());
        }

        public IQueryable<T> Table {
            get { return _dictionary.Values.AsQueryable(); }
        }

        public int Count(Expression<Func<T, bool>> predicate) {
            return _dictionary.Values.Count(predicate.Compile());
        }

        public IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate) {
            return _dictionary.Values.Where(predicate.Compile());
        }

        public IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order) {
            var orderable = new Orderable<T>(Table);
            order(orderable);
            return orderable.Queryable;
        }

        public IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order, int skip, int count) {
            return Fetch(predicate, order).Skip(skip).Take(count);
        }

        private int GetId(T entity) {
            return (int)entity.GetType().GetProperty("Id").GetValue(entity, null);
        }

        private void SetId(T entity, int id) {
            entity.GetType().GetProperty("Id").SetValue(entity, id, null);
        }
    }
}