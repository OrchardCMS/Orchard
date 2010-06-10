using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Utility.Extensions;

namespace Orchard.Comments.Extensions {
    public static class HtmlHelperExtensions {
        public static MvcHtmlString CommentSummaryLinks(this HtmlHelper html, Localizer T, ContentItem item, int commentCount, int pendingCount) {
            var commentText = "";

            if (item.Id != 0) {
                var totalCommentCount = commentCount + pendingCount;
                var totalCommentText = T.Plural("1 comment", "{0} comments", totalCommentCount);
                if (totalCommentCount == 0) {
                    commentText += totalCommentText.ToString();
                }
                else {
                    commentText +=
                        html.ActionLink(
                            totalCommentText.ToString(),
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