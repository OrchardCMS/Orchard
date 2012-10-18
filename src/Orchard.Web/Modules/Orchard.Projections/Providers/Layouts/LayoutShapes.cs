using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Utility.Extensions;

namespace Orchard.Projections.Providers.Layouts {
    public class LayoutShapes : IDependency {
        public LayoutShapes() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Shape]
        public void Grid(dynamic Display, TextWriter Output, HtmlHelper Html, string Id, bool Horizontal, IEnumerable<dynamic> Items, int Columns, IEnumerable<string> Classes, IDictionary<string, string> Attributes, IEnumerable<string> RowClasses, IDictionary<string, string> RowAttributes, IEnumerable<string> CellClasses, IDictionary<string, string> CellAttributes) {
            if (Items == null)
                return;

            var items = Items.ToList();
            var itemsCount = items.Count;

            if (itemsCount < 1)
                return;
            
            var gridTag = GetTagBuilder("table", Id, Classes, Attributes);
            var rowTag = GetTagBuilder("tr", string.Empty, RowClasses, RowAttributes);
            var cellTag = GetTagBuilder("td", string.Empty, CellClasses, CellAttributes);

            Output.Write(gridTag.ToString(TagRenderMode.StartTag));

            // resolves which item to display in a specific cell
            Func<int, int, int> seekItem = (row, col) => row*Columns + col;
            int maxRows = (itemsCount - 1) / Columns + 1;
            int maxCols = Columns;
            
            if (!Horizontal) {
                seekItem = (row, col) => col*Columns + row;
                maxCols = maxRows;
                maxRows = Columns;
            }

            for(int row=0; row < maxRows; row++) {
                Output.Write(rowTag.ToString(TagRenderMode.StartTag));
                for (int col = 0; col < maxCols; col++) {
                    int index = seekItem(row, col);
                    Output.Write(cellTag.ToString(TagRenderMode.StartTag));
                    if (index < itemsCount) {
                        Output.Write(Display(items[index]));
                    }
                    else {
                        // fill an empty cell
                        Output.Write("&nbsp;");
                    }

                    Output.Write(cellTag.ToString(TagRenderMode.EndTag));
                }
                Output.Write(rowTag.ToString(TagRenderMode.EndTag));
            }

            Output.Write(gridTag.ToString(TagRenderMode.EndTag));
        }

        static TagBuilder GetTagBuilder(string tagName, string id, IEnumerable<string> classes, IDictionary<string, string> attributes) {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.MergeAttributes(attributes, false);
            foreach (var cssClass in classes ?? Enumerable.Empty<string>())
                tagBuilder.AddCssClass(cssClass);
            if (!string.IsNullOrWhiteSpace(id))
                tagBuilder.GenerateId(id);
            return tagBuilder;
        }

    }
}
