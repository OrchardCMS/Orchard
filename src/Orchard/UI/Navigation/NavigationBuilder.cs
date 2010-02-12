using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.Security.Permissions;

namespace Orchard.UI.Navigation {
    public class NavigationBuilder {
        IEnumerable<MenuItem> Contained { get; set; }

        public NavigationBuilder Add(string caption, string position, string url, Action<NavigationItemBuilder> itemBuilder) {
            var childBuilder = new NavigationItemBuilder();

            if (!string.IsNullOrEmpty(caption))
                childBuilder.Caption(caption);

            if (!string.IsNullOrEmpty(position))
                childBuilder.Position(position);

            if (!string.IsNullOrEmpty(url))
                childBuilder.Url(url);

            itemBuilder(childBuilder);
            Contained = (Contained ?? Enumerable.Empty<MenuItem>()).Concat(childBuilder.Build());
            return this;
        }

        public NavigationBuilder Add(string caption, string position, Action<NavigationItemBuilder> itemBuilder) {
            return Add(caption, position, null, itemBuilder);
        }
        public NavigationBuilder Add(string caption, Action<NavigationItemBuilder> itemBuilder) {
            return Add(caption, null, null, itemBuilder);
        }
        public NavigationBuilder Add(Action<NavigationItemBuilder> itemBuilder) {
            return Add(null, null, null, itemBuilder);
        }
        public NavigationBuilder Add(string caption, string position, string url) {
            return Add(caption, position, url, x=> { });
        }
        public NavigationBuilder Add(string caption, string position) {
            return Add(caption, position, null, x=> { });
        }
        public NavigationBuilder Add(string caption) {
            return Add(caption, null, null, x => { });
        }

        public IEnumerable<MenuItem> Build() {
            return (Contained ?? Enumerable.Empty<MenuItem>()).ToList();
        }
    }

    public class NavigationItemBuilder : NavigationBuilder {
        private readonly MenuItem _item;

        public NavigationItemBuilder() {
            _item = new MenuItem();
        }

        public NavigationItemBuilder Caption(string caption) {
            _item.Text = caption;
            return this;
        }

        public NavigationItemBuilder Position(string position) {
            _item.Position = position;
            return this;
        }

        public NavigationItemBuilder Url(string url) {
            _item.Url = url;
            return this;
        }

        public NavigationItemBuilder Permission(Permission permission) {
            _item.Permissions = _item.Permissions.Concat(new[]{permission});
            return this;
        }

        public new IEnumerable<MenuItem> Build() {
            _item.Items = base.Build();
            return new[] { _item };
        }

        public NavigationItemBuilder Action(string actionName) {
            return Action(actionName, null, new RouteValueDictionary());
        }

        public NavigationItemBuilder Action(string actionName, string controllerName) {
            return Action(actionName, controllerName, new RouteValueDictionary());
        }

        public NavigationItemBuilder Action(string actionName, string controllerName, object values) {
            return Action(actionName, controllerName, new RouteValueDictionary(values));
        }

        public NavigationItemBuilder Action(string actionName, string controllerName, RouteValueDictionary values) {            
            _item.RouteValues = new RouteValueDictionary(values);
            if (!string.IsNullOrEmpty(actionName))
                _item.RouteValues["action"] = actionName;
            if (!string.IsNullOrEmpty(controllerName))
                _item.RouteValues["controller"] = controllerName;
            return this;
        }
    }
}