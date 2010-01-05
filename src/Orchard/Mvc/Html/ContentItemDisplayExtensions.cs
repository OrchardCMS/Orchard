using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.Mvc.ViewModels;

namespace Orchard.Mvc.Html {
    public static class ContentItemDisplayExtensions {
        public static MvcHtmlString DisplayForItem<TModel, TItemModel>(this HtmlHelper<TModel> html, TItemModel item) where TItemModel : ContentItemViewModel {
            return html.DisplayForItem(x => item);
        }

        public static MvcHtmlString DisplayForItem<TModel, TItemModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, TItemModel>> expression) where TItemModel : ContentItemViewModel {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var model = (TItemModel)metadata.Model;

            if (model.Adaptor != null) {
                return model.Adaptor(html, model).DisplayForModel(model.TemplateName, model.Prefix ?? "");
            }

            return html.DisplayFor(expression, model.TemplateName, model.Prefix ?? "");
        }
    }
}