using System;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeJournalManager : IRecipeJournal {
        private readonly IStorageProvider _storageProvider;

        public RecipeJournalManager(IStorageProvider storageProvider) {
            _storageProvider = storageProvider;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        ILogger Logger { get; set; }

        public void StartExecution(string executionId) {
            throw new NotImplementedException();
        }

        public void ExecutionComplete(string executionId) {
            throw new NotImplementedException();
        }

        public void ExecutionFailed(string executionId) {
            throw new NotImplementedException();
        }

        public void WriteJournalEntry(string executionId, string message) {
            throw new NotImplementedException();
        }

        public RecipeJournal GetRecipeJournal(string executionId) {
            throw new NotImplementedException();
        }

        private IStorageFile GetJournalFile(string executionId) {
            IStorageFile journalFile;
            try {
                journalFile = _storageProvider.GetFile(executionId);
            }
            catch (ArgumentException) {
                journalFile = _storageProvider.CreateFile(executionId);
            }

            return journalFile;
        }
    }
}