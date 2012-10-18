namespace Orchard.AntiSpam.Settings {
    public class SubmissionLimitPartSettings {
        public int Limit { get; set; }
        public int Unit { get; set; }
    }

    public enum SubmissionLimitUnit {
        Hour,
        Day,
        Month,
        Year,
        Overall
    }
}