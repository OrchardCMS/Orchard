using System;
using System.Web;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using Joel.Net;

namespace Orchard.Comments.Services {
    //This uses an akismet api implementation from http://akismetapi.codeplex.com/ 
    //Since the implementation is trivial, it may make sense to implement it to reduce dependencies.
    [UsedImplicitly]
    public class AkismetCommentValidator : ICommentValidator {
        private readonly INotifier _notifer;
        private readonly IOrchardServices _orchardServices;

        public AkismetCommentValidator(INotifier notifier, IOrchardServices orchardServices) {
            _notifer = notifier;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public bool ValidateComment(CommentPart commentPart) {
            CommentSettingsPartRecord commentSettingsPartRecord = _orchardServices.WorkContext.CurrentSite.As<CommentSettingsPart>().Record;
            string akismetKey = commentSettingsPartRecord.AkismetKey;
            string akismetUrl = commentSettingsPartRecord.AkismetUrl;
            bool enableSpamProtection = commentSettingsPartRecord.EnableSpamProtection;
            if (enableSpamProtection == false) {
                return true;
            }
            if (String.IsNullOrEmpty(akismetKey)) {
                _notifer.Information(T("Please configure your Akismet key for spam protection"));
                return true;
            }
            if (String.IsNullOrEmpty(akismetUrl)) {
                akismetUrl = "http://www.orchardproject.net";
            }
            Akismet akismetApi = new Akismet(akismetKey, akismetUrl, null);
            AkismetComment akismetComment = new AkismetComment {
                CommentAuthor = commentPart.Record.Author,
                CommentAuthorEmail = commentPart.Record.Email,
                Blog = akismetUrl,
                CommentAuthorUrl = commentPart.Record.SiteName,
                CommentContent = commentPart.Record.CommentText,
                UserAgent = HttpContext.Current.Request.UserAgent,
            };

            if (akismetApi.VerifyKey()) {
                return !akismetApi.CommentCheck(akismetComment); // CommentCheck returning true == spam
            }

            return false;
        }
    }
}