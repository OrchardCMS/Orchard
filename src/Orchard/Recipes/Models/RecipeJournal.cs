using System.Collections.Generic;

namespace Orchard.Recipes.Models {
    public class RecipeJournal {
        public string ExecutionId { get; set; }
        public RecipeStatus Status { get; set; }
        public IEnumerable<JournalMessage> Messages { get; set; }
    }

    public class JournalMessage {
        public string Message { get; set; }
    }

    public enum RecipeStatus {
        Unknown,
        Started,
        Complete,
        Failed
    }
}
