namespace Orchard.AntiSpam.Settings {
    public class SpamFilterPartSettings {
        public SpamFilterAction Action { get; set; }
        public bool DeleteSpam { get; set; }

        public string UrlPattern { get; set; }
        public string PermalinkPattern { get; set; }
        public string CommentAuthorPattern { get; set; }
        public string CommentAuthorEmailPattern { get; set; }
        public string CommentAuthorUrlPattern { get; set; }
        public string CommentContentPattern { get; set; }
    }

    /// <summary>
    /// The action to take when spam filters occur
    /// </summary>
    public enum SpamFilterAction {
        One, // Mark as spam if at least one declares spam
        AllOrNothing // Mark as spam if all provider declare spam
    }
}