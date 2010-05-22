using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.Comments.Extensions {
    public static class HtmlHelperExtensions {
        public static MvcHtmlString CommentSummaryLinks(this HtmlHelper html, Localizer T, ContentItem item, int commentCount, int pendingCount) {
            var commentText = "";

            if (item.Id != 0) {
                var totalCommentCount = commentCount + pendingCount;

                if (totalCommentCount == 0) {
                    commentText += html.Encode(T("0 comments"));
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
                                returnUrl = html.ViewContext.HttpContext.Request.ToUrlString()
                            });
                }

                if (pendingCount > 0) {
                    commentText += " " + html.ActionLink(T("({0} pending)", pendingCount).ToString(),
                                                   "Details",
                                                   new {
                                                       Area = "Orchard.Comments",
                                                       Controller = "Admin",
                                                       id = item.Id,
                                                       returnUrl = html.ViewContext.HttpContext.Request.Url
                                                   });
                }
            }

            return MvcHtmlString.Create(commentText);
        }
    }
}