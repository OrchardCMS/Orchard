using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Projections.FilterEditors {
    public class DefaultFilterFormater : IFilterCoordinator {
        private readonly IEnumerable<IFilterEditor> _filterEditors;

        public DefaultFilterFormater(IEnumerable<IFilterEditor> filterEditors) {
            _filterEditors = filterEditors;
        }

        public string GetForm(Type type) {
            var filterEditor = GetFilterEditor(type);
            if (filterEditor == null) {
                return null;
            }

            return filterEditor.FormName;
        }

        public Action<IHqlExpressionFactory> Filter(Type type, string property, dynamic formState) {
            var filterEditor = GetFilterEditor(type);
            if (filterEditor == null) {
                return x => { };
            }

            return filterEditor.Filter(property, formState);
        }

        public LocalizedString Display(Type type, string property, dynamic formState) {
            var filterEditor = GetFilterEditor(type);
            if (filterEditor == null) {
                return new LocalizedString(property);
            }

            return filterEditor.Display(property, formState);
        }

        private IFilterEditor GetFilterEditor(Type type) {
            return _filterEditors.FirstOrDefault(x => x.CanHandle(type));
        }
    }
}