using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.Models.ViewModels;

namespace Orchard.Mvc.Html {
    public static class ItemEditorExtensions {
        public static MvcHtmlString EditorForItem<TModel, TItemViewModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, TItemViewModel>> expression) where TItemViewModel : ItemEditorViewModel {

            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var model = (TItemViewModel)metadata.Model;

            return html.EditorFor(expression, model.TemplateName, model.Prefix ?? "");
        }


        public static MvcHtmlString EditorZone<TModel>(this HtmlHelper<TModel> html, string zoneName) where TModel : ItemEditorViewModel {
            var templates = html.ViewData.Model.Editors.Where(x => x.ZoneName == zoneName && x.WasUsed == false);
            return EditorZoneImplementation(html, templates);
        }

        public static MvcHtmlString EditorZonesAny<TModel>(this HtmlHelper<TModel> html) where TModel : ItemEditorViewModel {
            var templates = html.ViewData.Model.Editors.Where(x => x.WasUsed == false);
            return EditorZoneImplementation(html, templates);
        }

        public static MvcHtmlString EditorZones<TModel>(this HtmlHelper<TModel> html, params string[] include) where TModel : ItemEditorViewModel {
            var templates = html.ViewData.Model.Editors.Where(x => include.Contains(x.ZoneName) && x.WasUsed == false);
            return EditorZoneImplementation(html, templates);
        }

        public static MvcHtmlString EditorZonesExcept<TModel>(this HtmlHelper<TModel> html, params string[] exclude) where TModel : ItemEditorViewModel {
            var templates = html.ViewData.Model.Editors.Where(x => !exclude.Contains(x.ZoneName) && x.WasUsed == false);
            return EditorZoneImplementation(html, templates);
        }

        private static MvcHtmlString EditorZoneImplementation<TModel>(HtmlHelper<TModel> html, IEnumerable<TemplateViewModel> templates) {
            var count = templates.Count();
            if (count == 0)
                return null;

            if (count == 1) {
                var t = templates.Single();
                t.WasUsed = true;
                return html.EditorFor(m => t.Model, t.TemplateName, t.Prefix ?? "");
            }

            var strings = new List<MvcHtmlString>();
            foreach (var template in templates) {
                var t = template;
                t.WasUsed = true;
                strings.Add(html.EditorFor(m => t.Model, t.TemplateName, t.Prefix ?? ""));
            }
            return MvcHtmlString.Create(string.Concat(strings.ToArray()));
        }
 
    }
}