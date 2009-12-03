using System;
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
    }
}