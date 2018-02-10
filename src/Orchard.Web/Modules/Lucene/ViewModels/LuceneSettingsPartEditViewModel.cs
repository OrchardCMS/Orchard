using Lucene.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lucene.ViewModels {
    public class LuceneSettingsPartEditViewModel {
        public LuceneAnalyzerSelectorMapping[] LuceneAnalyzerSelectorMappings { get; set; }

        public IEnumerable<SelectListItem> LuceneAnalyzerSelectors { get; set; }
    }
}