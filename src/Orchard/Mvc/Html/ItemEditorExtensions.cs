using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Mvc.Html {
    public static class ItemEditorExtensions {
        public static MvcHtmlString EditorForItem<TModel, TItemModel>(this HtmlHelper<TModel> html, TItemModel item) where TItemModel : ItemEditorModel {
            return html.EditorForItem(x => item);
        }
        public static MvcHtmlString EditorForItem<TModel, TItemModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, TItemModel>> expression) where TItemModel : ItemEditorModel {

            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var model = (TItemModel)metadata.Model;

            if (model.Adaptor != null) {
                return model.Adaptor(html, model).EditorForModel(model.TemplateName, model.Prefix ?? "");
            }
            
            return html.EditorFor(expression, model.TemplateName, model.Prefix ?? "");
        }
    }
}