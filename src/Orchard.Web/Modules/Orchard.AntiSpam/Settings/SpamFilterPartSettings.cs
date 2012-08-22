namespace Orchard.AntiSpam.Settings {
    public class SpamFilterPartSettings {
        public SpamFilterAction Action { get; set; }
        public string Pattern { get; set; }
    }

    /// <summary>
    /// The action to take when spam filters occur
    /// </summary>
    public enum SpamFilterAction {
        AllOrNothing, // Mark as spam if all provider declare spam
        One // Mark as spam if at least one declares spam
    }
}