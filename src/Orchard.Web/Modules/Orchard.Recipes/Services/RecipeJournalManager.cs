using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeJournalManager : IRecipeJournal {
        private readonly IStorageProvider _storageProvider;
        private const string RecipeJournalFolder = "RecipeJournal";
        private const string WebConfig = 
@"
<configuration>
  <system.web>
    <httpHandlers>
      <clear />
      <add path=""*"" verb=""*"" type=""System.Web.HttpNotFoundHandler""/>
    </httpHandlers>
  </system.web>
  <system.webServer>
    <handlers accessPolicy=""Script"">
      <clear/>
      <add name=""NotFound"" path=""*"" verb=""*"" type=""System.Web.HttpNotFoundHandler"" preCondition=""integratedMode"" requireAccess=""Script""/>
    </handlers>
  </system.webServer>
</configuration>";

        public RecipeJournalManager(IStorageProvider storageProvider) {
            _storageProvider = storageProvider;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void ExecutionStart(string executionId) {
            var executionJournal = GetJournalFile(executionId);
            var xElement = XElement.Parse(ReadJournal(executionJournal));
            xElement.Element("Status").Value = "Started";
            WriteJournal(executionJournal, xElement);
        }

        public void ExecutionComplete(string executionId) {
            var executionJournal = GetJournalFile(executionId);
            var xElement = XElement.Parse(ReadJournal(executionJournal));
            xElement.Element("Status").Value = "Complete";
            WriteJournal(executionJournal, xElement);
        }

        public void ExecutionFailed(string executionId) {
            var executionJournal = GetJournalFile(executionId);
            var xElement = XElement.Parse(ReadJournal(executionJournal));
            xElement.Element("Status").Value = "Failed";
            WriteJournal(executionJournal, xElement);
        }

        public void WriteJournalEntry(string executionId, string message) {
            var executionJournal = GetJournalFile(executionId);
            var xElement = XElement.Parse(ReadJournal(executionJournal));
            var journalEntry = new XElement("Message", message);
            xElement.Add(journalEntry);
            WriteJournal(executionJournal, xElement);
        }

        public RecipeJournal GetRecipeJournal(string executionId) {
            var executionJournal = GetJournalFile(executionId);
            var xElement = XElement.Parse(ReadJournal(executionJournal));

            var journal = new RecipeJournal { ExecutionId = executionId };
            var messages = new List<JournalMessage>();

            journal.Status = ReadStatusFromJournal(xElement);
            foreach (var message in xElement.Elements("Message")) {
                messages.Add(new JournalMessage {Message = message.Value});
            }
            journal.Messages = messages;

            return journal;
        }

        public RecipeStatus GetRecipeStatus(string executionId) {
            var executionJournal = GetJournalFile(executionId);
            var xElement = XElement.Parse(ReadJournal(executionJournal));

            return ReadStatusFromJournal(xElement);
        }

        private IStorageFile GetJournalFile(string executionId) {
            IStorageFile journalFile;
            var journalPath = _storageProvider.Combine(RecipeJournalFolder, executionId);
            try {
                if (_storageProvider.TryCreateFolder(RecipeJournalFolder)) {
                    var webConfigPath = _storageProvider.Combine(RecipeJournalFolder, "web.config");
                    var webConfigFile = _storageProvider.CreateFile(webConfigPath);
                    WriteWebConfig(webConfigFile);
                }
                journalFile = _storageProvider.GetFile(journalPath);
            }
            catch (ArgumentException) {
                journalFile = _storageProvider.CreateFile(journalPath);
                var recipeStepElement = new XElement("RecipeJournal");
                recipeStepElement.Add(new XElement("Status", "Unknown"));
                WriteJournal(journalFile, recipeStepElement);
            }

            return journalFile;
        }

        private static string ReadJournal(IStorageFile executionJournal) {
            using (var stream = executionJournal.OpenRead()) {
                using (var streamReader = new StreamReader(stream)) {
                    return streamReader.ReadToEnd();
                }
            } 
        }

        private static void WriteJournal(IStorageFile journalFile, XElement journal) {
            string content = journal.ToString();
            using (var stream = journalFile.CreateFile()) {
                using (var tw = new StreamWriter(stream)) {
                    tw.Write(content);
                }
            }
        }

        private static void WriteWebConfig(IStorageFile webConfigFile) {
            using (var stream = webConfigFile.OpenWrite()) {
                using (var tw = new StreamWriter(stream)) {
                    tw.Write(WebConfig);
                }
            }
        }

        private static RecipeStatus ReadStatusFromJournal(XElement xElement) {
            switch (xElement.Element("Status").Value) {
                case "Started":
                    return RecipeStatus.Started;
                case "Complete":
                    return RecipeStatus.Complete;
                case "Failed":
                    return RecipeStatus.Failed;
                default:
                    return RecipeStatus.Unknown;
            }
        }
    }
}