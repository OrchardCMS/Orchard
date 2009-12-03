using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.Models.ViewModels;

namespace Orchard.Mvc.Html {
    public static class ItemDisplayExtensions {
        public static MvcHtmlString DisplayForItem<TModel, TItemViewModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, TItemViewModel>> expression) where TItemViewModel : ItemDisplayViewModel {

            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var model = (TItemViewModel)metadata.Model;

            return html.DisplayFor(expression, model.TemplateName, model.Prefix ?? "");
        }
    }
}