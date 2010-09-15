using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentPartDriver : ContentPartDriver<CommentPart> {
        protected override string Prefix { get { return "Comments"; } }
    }
}