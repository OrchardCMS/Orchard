using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Comments.Models;
using Orchard.Comments.Settings;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.State;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Messaging.Services;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Comments.Services {
    public class CommentService : ICommentService {
        private readonly IOrchardServices _orchardServices;
        private readonly IClock _clock;
        private readonly IEncryptionService _encryptionService;
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly HashSet<int> _processedCommentsParts = new HashSet<int>();
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly IMessageService _messageService;

        public CommentService(
            IOrchardServices orchardServices, 
            IClock clock, 
            IEncryptionService encryptionService,
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay,
            IMessageService messageService
            ) {
            _orchardServices = orchardServices;
            _clock = clock;
            _encryptionService = encryptionService;
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;
            _messageService = messageService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; } 
        public ILogger Logger { get; set; }

        public IContentQuery<CommentPart, CommentPartRecord> GetCommentsForContainer(int id) {
            return GetComments()
                .Where(c => c.CommentedOnContainer == id);
        }

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
                       .Where(c => c.CommentedOn == id);
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

        public void ProcessCommentsCount(int commentsPartId) {
            if (!_processedCommentsParts.Contains(commentsPartId)) {
                _processedCommentsParts.Add(commentsPartId);
                _processingEngine.AddTask(_shellSettings, _shellDescriptorManager.GetShellDescriptor(), "ICommentsCountProcessor.Process", new Dictionary<string, object> { { "commentsPartId", commentsPartId } });
            }
        }

        public void ApproveComment(int commentId) {
            var commentPart = GetCommentWithQueryHints(commentId);
            commentPart.Record.Status = CommentStatus.Approved;
            ProcessCommentsCount(commentPart.CommentedOn);
        }

        public void UnapproveComment(int commentId) {
            var commentPart = GetCommentWithQueryHints(commentId);
            commentPart.Record.Status = CommentStatus.Pending;
            ProcessCommentsCount(commentPart.CommentedOn);
        }

        public void DeleteComment(int commentId) {
            // Get latest because the comment may be unpublished if the anti-spam module has caught it
            _orchardServices.ContentManager.Remove(_orchardServices.ContentManager.Get<CommentPart>(commentId, VersionOptions.Latest).ContentItem);
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

        public bool CanCreateComment(CommentPart commentPart) {
            if (commentPart == null) {
                return false;
            }

            var container = _orchardServices.ContentManager.Get(commentPart.CommentedOn);
            
            if (container == null) {
                return false;
            }

            var commentsPart = container.As<CommentsPart>();
            if (commentsPart == null) {
                return false;
            }
            
            var settings = commentsPart.TypePartDefinition.Settings.GetModel<CommentsPartSettings>();
            if (!commentsPart.CommentsActive) {
                return false;
            }

            if (settings.MustBeAuthenticated && _orchardServices.WorkContext.CurrentUser == null) {
                return false;
            }
            
            if (!CanStillCommentOn(commentsPart)) {
                return false;
            }

            return true;
        }

        public bool CanStillCommentOn(CommentsPart commentsPart) {
            var commentSettings = _orchardServices.WorkContext.CurrentSite.As<CommentSettingsPart>();
            if (commentSettings == null) {
                return false;
            }

            if (commentSettings.ClosedCommentsDelay > 0) {
                var commonPart = commentsPart.As<CommonPart>();
                if (commentsPart == null) {
                    return false;
                }

                if (!commonPart.CreatedUtc.HasValue) {
                    return false;
                }

                if (commonPart.CreatedUtc.Value.AddDays(commentSettings.ClosedCommentsDelay) < _clock.UtcNow) {
                    return false;
                }
            }

            return true;
        }

        public void SendNotificationEmail(CommentPart commentPart) {
            try {
                var commentedOn = _orchardServices.ContentManager.Get(commentPart.CommentedOn);
                if (commentedOn == null) {
                    return;
                }

                var owner = commentedOn.As<CommonPart>().Owner;
                if (owner == null) {
                    return;
                }

                var template = _shapeFactory.Create("Template_Comment_Notification", Arguments.From(new {
                    CommentPart = commentPart,
                    CommentApproveUrl = CreateProtectedUrl("Approve", commentPart),
                    CommentModerateUrl = CreateProtectedUrl("Moderate", commentPart),
                    CommentDeleteUrl = CreateProtectedUrl("Delete", commentPart)
                }));

                var parameters = new Dictionary<string, object> {
                        {"Subject", T("Comment notification").Text},
                        {"Body", _shapeDisplay.Display(template)},
                        {"Recipients", owner.Email}
                    };

                _messageService.Send("Email", parameters);
            }
            catch(Exception e) {
                Logger.Error(e, "An unexpected error occurred while sending a notification email");
            }
        }

        public string CreateProtectedUrl(string action, CommentPart part) {
            var workContext = _orchardServices.WorkContext;
            if (workContext.HttpContext != null) {
                var url = new UrlHelper(workContext.HttpContext.Request.RequestContext);
                return url.AbsoluteAction(action, "Comment", new { area = "Orchard.Comments", nonce = CreateNonce(part, TimeSpan.FromDays(7)) });
            }

            return null;
        }

        private CommentPart GetCommentWithQueryHints(int id) {
            return _orchardServices.ContentManager.Get<CommentPart>(id, VersionOptions.Latest, new QueryHints().ExpandParts<CommentPart>());
        }
    }
}
