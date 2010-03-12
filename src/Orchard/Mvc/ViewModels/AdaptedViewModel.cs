using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.UI.Zones;

namespace Orchard.Mvc.ViewModels {
    class AdaptedViewModel : BaseViewModel {
        private readonly Property<ZoneCollection> _zones;
        private readonly Property<IEnumerable<MenuItem>> _menu;
        private readonly Property<IList<NotifyEntry>> _messages;
        private readonly Property<IUser> _currentUser;

        public AdaptedViewModel(ViewDataDictionary original) {
            _zones = new Property<ZoneCollection>(original, "Zones", () => new ZoneCollection());
            _menu = new Property<IEnumerable<MenuItem>>(original, "Menu", Enumerable.Empty<MenuItem>);
            _messages = new Property<IList<NotifyEntry>>(original, "Messages", () => new NotifyEntry[0]);
            _currentUser = new Property<IUser>(original, "CurrentUser", () => null);
        }

        public override ZoneCollection Zones { get { return _zones.Value; } set { _zones.Value = value; } }
        public override IEnumerable<MenuItem> Menu { get { return _menu.Value; } set { _menu.Value = value; } }
        public override IList<NotifyEntry> Messages { get { return _messages.Value; } set { _messages.Value = value; } }
        public override IUser CurrentUser { get { return _currentUser.Value; } set { _currentUser.Value = value; } }


        class Property<TProperty> where TProperty : class {
            private readonly ViewDataDictionary _original;
            private readonly string _expression;
            private Func<TProperty> _builder;
            private TProperty _value;

            public Property(ViewDataDictionary original, string expression, Func<TProperty> builder) {
                _original = original;
                _expression = expression;
                _builder = builder;
            }

            public TProperty Value {
                get {
                    if (_value == null && _builder != null) {
                        object temp;
                        if (_original.TryGetValue(_expression, out temp) &&
                            temp is TProperty) {
                            SetValue(temp as TProperty);
                        }
                        else {
                            SetValue(_builder());
                        }
                    }
                    return _value;
                }
                set { SetValue(value); }
            }

            private void SetValue(TProperty value) {
                _builder = null;
                _value = value;
                _original[_expression] = _value;
            }
        }
    }
}