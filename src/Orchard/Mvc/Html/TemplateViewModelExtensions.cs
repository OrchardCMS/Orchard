using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.ContentManagement.ViewModels;
using Orchard.UI;

namespace Orchard.Mvc.Html {
    public static class TemplateViewModelExtensions {
        public static void RenderTemplates(this HtmlHelper html, IEnumerable<TemplateViewModel> templates) {
            if (templates == null)
                return;

            foreach (var template in templates.OrderByDescending(t => t.Position, new FlatPositionComparer())) {
                html.RenderTemplates(template);
            }
        }

        public static void RenderTemplates(this HtmlHelper html, TemplateViewModel template) {
            if (template.WasUsed)
                return;

            template.WasUsed = true;

            var templateInfo = html.ViewContext.ViewData.TemplateInfo;
            var htmlFieldPrefix = templateInfo.HtmlFieldPrefix;
            try {
                templateInfo.HtmlFieldPrefix = templateInfo.GetFullHtmlFieldName(template.Prefix);
                html.RenderPartial(template.TemplateName, template.Model);
            }
            finally {
                templateInfo.HtmlFieldPrefix = htmlFieldPrefix;
            }
        }
    }
}