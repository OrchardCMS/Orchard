using System;
using Orchard.Comments.Models;
using Orchard.Events;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Comments.Rules {
    public interface IActionProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    public class CommentsActions : IActionProvider {
        private readonly IContentManager _contentManager;

        public CommentsActions(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            Func<dynamic, LocalizedString> display = context => T("Close comments");

            describe.For("Comments", T("Comments"), T("Comments"))
                .Element("Close", T("Close Comments"), T("Closes comments on a content item."), (Func<dynamic, bool>)Close, display, "ActionCloseComments");
        }

        /// <summary>
        /// Closes the comments on the content represented by "ContentId"
        /// </summary>
        private bool Close(dynamic context) {
            var contentId = Convert.ToInt32(context.Properties["ContentId"]);
            ContentItem content = _contentManager.Get(contentId);

            if (content != null) {
                var comments = content.As<CommentsPart>();
                if (comments != null) {
                    comments.CommentsActive = false;
                }
            }

            return true;
        }
    }
}