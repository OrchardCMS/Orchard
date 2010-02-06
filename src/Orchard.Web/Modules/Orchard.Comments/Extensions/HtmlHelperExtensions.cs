using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Comments.Extensions {
    public static class HtmlHelperExtensions {
        public static MvcHtmlString CommentSummaryLinks(this HtmlHelper html, Localizer T, ContentItem item, int commentCount, int pendingCount) {
            string commentText = "";

            if (item.Id != 0) {
                //
                int totalCommentCount = commentCount + pendingCount;

                if (totalCommentCount == 0) {
                    commentText += html.Encode(T("no comments"));
                }
                else {
                    commentText +=
                        html.ActionLink(
                            T("{0} comment{1}", totalCommentCount, totalCommentCount == 1 ? "" : "s").ToString(),
                            "Details",
                            new {
                                Area = "Orchard.Comments",
                                Controller = "Admin",
                                id = item.Id,
                                returnUrl = html.ViewContext.HttpContext.Request.Url
                            });
                }

                if (pendingCount > 0) {
                    commentText += " (";
                    commentText += html.ActionLink(T("{0} pending", pendingCount).ToString(),
                                                   "Details",
                                                   new {
                                                       Area = "Orchard.Comments",
                                                       Controller = "Admin",
                                                       id = item.Id,
                                                       returnUrl = html.ViewContext.HttpContext.Request.Url
                                                   });
                    commentText += ") ";
                }
            }

            return MvcHtmlString.Create(commentText);
        }
    }
}