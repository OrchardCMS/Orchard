using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.Models.ViewModels;

namespace Orchard.Mvc.Html {
    public static class ItemDisplayExtensions {
        public static MvcHtmlString DisplayForItem<TModel, TItemViewModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, TItemViewModel>> expression) where TItemViewModel : ItemDisplayViewModel {

            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var model = (TItemViewModel)metadata.Model;

            if (model.Adaptor != null) {
                return model.Adaptor(html, model).DisplayForModel(model.TemplateName, model.Prefix ?? "");
            }
            
            return html.DisplayFor(expression, model.TemplateName, model.Prefix ?? "");
        }


        public static MvcHtmlString DisplayZone<TModel>(this HtmlHelper<TModel> html, string zoneName) where TModel : ItemDisplayViewModel {
            var templates = html.ViewData.Model.Displays.Where(x => x.ZoneName == zoneName && x.WasUsed == false);
            return DisplayZoneImplementation(html, templates);
        }

        public static MvcHtmlString DisplayZonesAny<TModel>(this HtmlHelper<TModel> html) where TModel : ItemDisplayViewModel {
            var templates = html.ViewData.Model.Displays.Where(x => x.WasUsed == false);
            return DisplayZoneImplementation(html, templates);
        }

        public static MvcHtmlString DisplayZones<TModel>(this HtmlHelper<TModel> html, params string[] include) where TModel : ItemDisplayViewModel {
            var templates = html.ViewData.Model.Displays.Where(x => include.Contains(x.ZoneName) && x.WasUsed == false);
            return DisplayZoneImplementation(html, templates);
        }

        public static MvcHtmlString DisplayZonesExcept<TModel>(this HtmlHelper<TModel> html, params string[] exclude) where TModel : ItemDisplayViewModel {
            var templates = html.ViewData.Model.Displays.Where(x => !exclude.Contains(x.ZoneName) && x.WasUsed == false);
            return DisplayZoneImplementation(html, templates);
        }

        private static MvcHtmlString DisplayZoneImplementation<TModel>(HtmlHelper<TModel> html, IEnumerable<TemplateViewModel> templates) {
            var count = templates.Count();
            if (count == 0)
                return null;

            if (count == 1) {
                var t = templates.Single();
                t.WasUsed = true;
                return html.DisplayFor(m => t.Model, t.TemplateName, t.Prefix ?? "");
            }

            var strings = new List<MvcHtmlString>();
            foreach (var template in templates) {
                var t = template;
                t.WasUsed = true;
                strings.Add(html.DisplayFor(m => t.Model, t.TemplateName, t.Prefix ?? ""));
            }
            return MvcHtmlString.Create(string.Concat(strings.ToArray()));
        }
    }
}