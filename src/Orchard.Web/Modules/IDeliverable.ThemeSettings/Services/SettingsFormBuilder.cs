using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Forms.Services;
using Orchard.UI;
using IDeliverable.ThemeSettings.Models;

namespace IDeliverable.ThemeSettings.Services
{
    public class SettingsFormBuilder : Component, ISettingsFormBuilder
    {
        private readonly IShapeFactory _shapeFactory;
        private readonly ITagBuilderFactory _tagBuilderFactory;
        private readonly IFormManager _formManager;
        private readonly Lazy<IDictionary<string, IThemeSettingProvider>> _providers;

        public SettingsFormBuilder(IShapeFactory shapeFactory, ITagBuilderFactory tagBuilderFactory, IFormManager formManager, Lazy<IEnumerable<IThemeSettingProvider>> providers)
        {
            _shapeFactory = shapeFactory;
            _tagBuilderFactory = tagBuilderFactory;
            _formManager = formManager;

            _providers = new Lazy<IDictionary<string, IThemeSettingProvider>>(() => InitializeProvidersDictionary(providers));
        }

        private dynamic New
        {
            get { return _shapeFactory; }
        }

        public dynamic BuildForm(ThemeSettingsManifest manifest)
        {
            var form = New.ThemeProfileSettingsForm();
            var providers = _providers.Value;

            foreach (var group in manifest.Groups)
            {
                var fieldset = New.Fieldset().Title(group.Name);

                foreach (var setting in group.Settings)
                {
                    var provider = providers.ContainsKey(setting.Type) ? providers[setting.Type] : providers["Default"];
                    var input = provider.BuildEditor(New, setting);

                    fieldset.Add(input);
                }

                form.Add(fieldset);
            }

            return form;
        }

        public dynamic BindForm(dynamic form, IValueProvider valueProvider)
        {
            return _formManager.Bind(form, valueProvider);
        }

        [Shape]
        public void ThemeProfileSettingsForm(Action<object> Output, dynamic Display, dynamic Shape)
        {
            var tag = _tagBuilderFactory.Create(Shape, "section");

            Output(tag.ToString(TagRenderMode.StartTag));
            foreach (var item in Ordered(Shape.Items))
            {
                Output(Display(item));
            }
            Output(tag.ToString(TagRenderMode.EndTag));
        }

        private IDictionary<string, IThemeSettingProvider> InitializeProvidersDictionary(Lazy<IEnumerable<IThemeSettingProvider>> providers)
        {
            return providers.Value.ToDictionary(x => x.TypeName);
        }

        private static IEnumerable<object> Ordered(IEnumerable<object> items)
        {
            return items.Select(Positionify).OrderBy(p => p.Item1, new FlatPositionComparer()).Select(p => p.Item2);
        }

        private static Tuple<string, object> Positionify(dynamic item, int naturalIndex)
        {
            var shape = item as IShape;
            if (shape != null)
            {
                return new Tuple<string, object>(shape.Metadata.Position, item);
            }
            if (item is Tuple<string, object>)
            {
                return item;
            }

            // non-shape items are given a position equal to their index in the list, giving the shapes a way of mingling among them
            return new Tuple<string, object>(naturalIndex.ToString(CultureInfo.InvariantCulture), item);
        }
    }
}