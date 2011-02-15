using System.Collections.Generic;

namespace Orchard.Recipes.Models {
    public class RecipeJournal {
        public string ExecutionId { get; set; }
        public RecipeJournalStatus Status { get; set; }
        public IEnumerable<JournalMessage> Messages { get; set; }
    }

    public class JournalMessage {
        public string Message { get; set; }
    }

    public enum RecipeJournalStatus {
        Running,
        Complete,
        Failed
    }
}
