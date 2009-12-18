using System.Collections.Generic;
using System.Linq;

namespace Orchard.UI.PageTitle {
    public class PageTitleBuilder : IPageTitleBuilder {
        private readonly List<string> _titleParts;
        private readonly string _titleSeparator;

        public PageTitleBuilder() {
            _titleParts = new List<string>(5);
            //TODO: (erikpo) Get this from a site setting
            _titleSeparator = " - ";
        }

        public void AddTitleParts(params string[] titleParts) {
            if (titleParts != null)
                foreach (string titlePart in titleParts)
                    if (!string.IsNullOrEmpty(titlePart))
                        _titleParts.Add(titlePart);
        }

        public string GenerateTitle() {
            return string.Join(_titleSeparator, _titleParts.AsEnumerable().Reverse().ToArray());
        }
    }
}