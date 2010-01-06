using System.Collections.Generic;
using System.IO;
using Orchard.Pages.Services.Templates;

namespace Orchard.Tests.Packages.Pages.Services.Templates {
    public class StubTemplateEntryProvider : ITemplateEntryProvider {
        private readonly List<TemplateEntry> _templates = new List<TemplateEntry>();

        public IEnumerable<TemplateEntry> List() {
            return _templates;
        }

        public void AddTemplate(string fileName, string fileContent) {
            _templates.Add(new TemplateEntry { Name = fileName, Content = new StringReader(fileContent) });
        }
    }
}