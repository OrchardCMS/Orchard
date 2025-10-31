using System.Collections.Generic;
using System.Web.Mvc;
using Lucene.Models;

namespace Lucene.ViewModels
{
    public class LuceneSettingsPartEditViewModel
    {
        public LuceneAnalyzerSelectorMapping[] LuceneAnalyzerSelectorMappings { get; set; }

        public IEnumerable<SelectListItem> LuceneAnalyzerSelectors { get; set; }
    }
}