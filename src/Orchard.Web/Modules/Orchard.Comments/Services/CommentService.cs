using System;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Comments.Services {
    [UsedImplicitly]
    public class CommentService : ICommentService {
        private readonly IOrchardServices _orchardServices;
        private readonly IClock _clock;
        private readonly IEncryptionService _encryptionService;

        public CommentService(
            IOrchardServices orchardServices, 
            IClock clock, 
            IEncryptionService encryptionService) {
            _orchardServices = orchardServices;
            _clock = clock;
            _encryptionService = encryptionService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public CommentPart GetComment(int id) {
            return _orchardServices.ContentManager.Get<CommentPart>(id);
        }
        
        public IContentQuery<CommentPart, CommentPartRecord> GetComments() {
            return _orchardServices.ContentManager
                       .Query<CommentPart, CommentPartRecord>();
        }

        public IContentQuery<CommentPart, CommentPartRecord> GetComments(CommentStatus status) {
            return GetComments()
                       .Where(c => c.Status == status);
        }

        public IContentQuery<CommentPart, CommentPartRecord> GetCommentsForCommentedContent(int id) {
            return GetComments()
                       .Where(c => c.CommentedOn == id || c.CommentedOnContainer == id);
        }

        public IContentQuery<CommentPart, CommentPartRecord> GetCommentsForCommentedContent(int id, CommentStatus status) {
            return GetCommentsForCommentedContent(id)
                       .Where(c => c.Status == status);
        }

        public ContentItemMetadata GetDisplayForCommentedContent(int id) {
            var content = GetCommentedContent(id);
            if (content == null)
                return null;
            return _orchardServices.ContentManager.GetItemMetadata(content);
        }

        public ContentItem GetCommentedContent(int id) {
            var result = _orchardServices.ContentManager.Get(id, VersionOptions.Published);
            if (result == null)
                result = _orchardServices.ContentManager.Get(id, VersionOptions.Draft);
            return result;
        }

        public void ApproveComment(int commentId) {
            var commentPart = GetCommentWithQueryHints(commentId);
            commentPart.Record.Status = CommentStatus.Approved;
        }

        public void UnapproveComment(int commentId) {
            var commentPart = GetCommentWithQueryHints(commentId);
            commentPart.Record.Status = CommentStatus.Pending;
        }

        public void DeleteComment(int commentId) {
            _orchardServices.ContentManager.Remove(_orchardServices.ContentManager.Get<CommentPart>(commentId).ContentItem);
        }

        public bool CommentsDisabledForCommentedContent(int id) {
            return !_orchardServices.ContentManager.Get<CommentsPart>(id, VersionOptions.Latest).CommentsActive;
        }

        public void DisableCommentsForCommentedContent(int id) {
            _orchardServices.ContentManager.Get<CommentsPart>(id, VersionOptions.Latest).CommentsActive = false;
        }

        public void EnableCommentsForCommentedContent(int id) {
            _orchardServices.ContentManager.Get<CommentsPart>(id, VersionOptions.Latest).CommentsActive = true;
        }

        public string CreateNonce(CommentPart comment, TimeSpan delay) {
            var challengeToken = new XElement("n", new XAttribute("c", comment.Id), new XAttribute("v", _clock.UtcNow.ToUniversalTime().Add(delay).ToString(CultureInfo.InvariantCulture))).ToString();
            var data = Encoding.UTF8.GetBytes(challengeToken);
            return Convert.ToBase64String(_encryptionService.Encode(data));
        }

        public bool DecryptNonce(string nonce, out int id) {
            id = 0;

            try {
                var data = _encryptionService.Decode(Convert.FromBase64String(nonce));
                var xml = Encoding.UTF8.GetString(data);
                var element = XElement.Parse(xml);
                id = Convert.ToInt32(element.Attribute("c").Value);
                var validateByUtc = DateTime.Parse(element.Attribute("v").Value, CultureInfo.InvariantCulture);
                return _clock.UtcNow <= validateByUtc;
            }
            catch {
                return false;
            }

        }
        private CommentPart GetCommentWithQueryHints(int id) {
            return _orchardServices.ContentManager.Get<CommentPart>(id, VersionOptions.Latest, new QueryHints().ExpandParts<CommentPart>());
        }
    }
}
