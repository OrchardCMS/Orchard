using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Settings;

namespace Orchard.UI.PageTitle {
    public class PageTitleBuilder : IPageTitleBuilder {
        private readonly ISiteService _siteService;
        private readonly List<string> _titleParts;
        private string _titleSeparator;

        public PageTitleBuilder(ISiteService siteService) {
            _siteService = siteService;
            _titleParts = new List<string>(5);
        }

        public void AddTitleParts(params string[] titleParts) {
            if (titleParts.Length > 0)
                foreach (string titlePart in titleParts)
                    if (!string.IsNullOrEmpty(titlePart))
                        _titleParts.Add(titlePart);
        }

        public void AppendTitleParts(params string[] titleParts) {
            if (titleParts.Length > 0)
                foreach (string titlePart in titleParts)
                    if (!string.IsNullOrEmpty(titlePart))
                        _titleParts.Insert(0, titlePart);
        }

        public string GenerateTitle() {
            if (_titleSeparator == null) {
                _titleSeparator = _siteService.GetSiteSettings().PageTitleSeparator;
            }

            return _titleParts.Count == 0 
                ? String.Empty
                : String.Join(_titleSeparator, _titleParts.AsEnumerable().Reverse().ToArray());
        }
    }
}