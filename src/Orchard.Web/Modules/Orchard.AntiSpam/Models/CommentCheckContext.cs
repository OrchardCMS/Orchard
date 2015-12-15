namespace Orchard.AntiSpam.Models {
    public class CommentCheckContext {
        /// <summary>
        /// The front page or home URL of the instance making the request. For a blog 
        /// or wiki this would be the front page. Note: Must be a full URI, including http://.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// IP address of the comment submitter.
        /// </summary>
        public string UserIp { get; set; }
        
        /// <summary>
        /// User agent string of the web browser submitting the comment - typically 
        /// the HTTP_USER_AGENT cgi variable. Not to be confused with the user agent 
        /// of your Akismet library.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// The content of the HTTP_REFERER header should be sent here.
        /// </summary>
        public string Referrer { get; set; }

        /// <summary>
        /// The permanent location of the entry the comment was submitted to.
        /// </summary>
        public string Permalink { get; set; }

        /// <summary>
        /// May be blank, comment, trackback, pingback, or a made up value like "registration".
        /// </summary>
        public string CommentType { get; set; }

        /// <summary>
        /// Name submitted with the comment
        /// </summary>
        public string CommentAuthor { get; set; }

        /// <summary>
        /// Email address submitted with the comment
        /// </summary>
        public string CommentAuthorEmail { get; set; }

        /// <summary>
        /// URL submitted with comment
        /// </summary>
        public string CommentAuthorUrl { get; set; }

        /// <summary>
        /// The content that was submitted.
        /// </summary>
        public string CommentContent { get; set; }
    }
}