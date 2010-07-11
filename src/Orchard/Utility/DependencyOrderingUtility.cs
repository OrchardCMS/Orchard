using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Utility {
    public static class DependencyOrdering {

        class Linkage<T> {
            public T Element { get; set; }
            public bool Used { get; set; }
        }

        /// <summary>
        /// Sort a collection of elements "by dependency order". By passing a lambda which determines if an element
        /// is a dependency of another, this algorithm will return the collection of elements sorted
        /// so that a given element in the sequence doesn't depend on any other element further in the sequence.
        /// </summary>
        public static IEnumerable<T> OrderByDependencies<T>(this IEnumerable<T> elements, Func<T, T, bool> hasDependency) {
            var population = elements.Select(d => new Linkage<T> {
                Element = d
            }).ToArray();

            var result = new List<T>();
            foreach (var item in population) {
                Add(item, result, population, hasDependency);
            }
            return result;
        }

        private static void Add<T>(Linkage<T> item, ICollection<T> list, IEnumerable<Linkage<T>> population, Func<T, T, bool> hasDependency) {
            if (item.Used)
                return;

            item.Used = true;
            foreach (var dependency in population.Where(dep => hasDependency(item.Element, dep.Element))) {
                Add(dependency, list, population, hasDependency);
            }
            list.Add(item.Element);
        }
    }
}