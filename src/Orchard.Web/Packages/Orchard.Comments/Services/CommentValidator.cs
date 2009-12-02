using System;
using System.Web;
using Orchard.Comments.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Settings;
using Orchard.UI.Notify;
using Joel.Net;

namespace Orchard.Comments.Services {
    //This uses an akismet api implementation from http://akismetapi.codeplex.com/ 
    //Since the implementation is trivial, it may make sense to implement it to reduce dependencies.
    public class AkismetCommentValidator : ICommentValidator {
        private readonly INotifier _notifer;
        public AkismetCommentValidator(INotifier notifier) {
            _notifer = notifier;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public ISite CurrentSite { get; set; }

        #region Implementation of ICommentValidator

        public bool ValidateComment(Comment comment) {
            CommentSettingsRecord commentSettingsRecord = CurrentSite.As<CommentSettings>().Record;
            string akismetKey = commentSettingsRecord.AkismetKey;
            string akismetUrl = commentSettingsRecord.AkismetUrl;
            bool enableSpamProtection = commentSettingsRecord.EnableSpamProtection;
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
                CommentAuthor = comment.Author,
                CommentAuthorEmail = comment.Email,
                Blog = comment.SiteName,
                CommentAuthorUrl = comment.SiteName,
                CommentContent = comment.CommentText,
                UserAgent = HttpContext.Current.Request.UserAgent,
            };

            if (akismetApi.VerifyKey()) {
                return akismetApi.CommentCheck(akismetComment);
            }

            return false;
        }

        #endregion
    }
}
