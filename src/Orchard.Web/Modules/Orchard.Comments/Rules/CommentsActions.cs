using System;
using Orchard.Comments.Models;
using Orchard.Environment.Extensions;
using Orchard.Rules.Models;
using Orchard.Rules.Services;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Comments.Rules
{
    [OrchardFeature("Orchard.Comments.Rules")]
    public class CommentsActions : IActionProvider
    {
        private readonly IContentManager _contentManager;

        public CommentsActions(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeActionContext context)
        {
            context.For("Comments", T("Comments"), T("Comments"))
                .Element("Close", T("Close Comments"), T("Closes comments on a content item."), Close, actionContext => T("Close comments"), "ActionCloseComments");
        }

        private void Close(ActionContext context)
        {
            var contentId = Convert.ToInt32(context.Properties["ContentId"]);
            var content = _contentManager.Get(contentId);

            if (content != null)
            {
                var comments = content.As<CommentsPart>();
                if (comments != null)
                {
                    comments.CommentsActive = false;
                }
            }
        }
    }
}