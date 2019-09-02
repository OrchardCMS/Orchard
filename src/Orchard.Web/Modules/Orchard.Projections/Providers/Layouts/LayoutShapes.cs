using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Localization;

namespace Orchard.Projections.Providers.Layouts {
    public class LayoutShapes : IDependency {
        public LayoutShapes() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Shape]
        public void Grid(dynamic Display, TextWriter Output, HtmlHelper Html, string Id, bool Horizontal, IEnumerable<dynamic> Items, int Columns, string Tag, IEnumerable<string> Classes, IDictionary<string, string> Attributes, string RowTag, IEnumerable<string> RowClasses, IDictionary<string, string> RowAttributes, string CellTag, IEnumerable<string> CellClasses, IDictionary<string, string> CellAttributes, string EmptyCell) {
            if (Items == null)
                return;

            var items = Items.ToList();
            var itemsCount = items.Count;

            if (itemsCount < 1)
                return;

            var gridTag = String.IsNullOrEmpty(Tag) ? null : GetTagBuilder(Tag, Id, Classes, Attributes);
            var rowTag = String.IsNullOrEmpty(RowTag) ? null : GetTagBuilder(RowTag, string.Empty, RowClasses, RowAttributes);
            var cellTag = String.IsNullOrEmpty(CellTag) ? null : GetTagBuilder(CellTag, string.Empty, CellClasses, CellAttributes);

            if (gridTag != null) {
                Output.Write(gridTag.ToString(TagRenderMode.StartTag));
            }

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

                if (rowTag != null) {
                    Output.Write(rowTag.ToString(TagRenderMode.StartTag));
                }

                for (int col = 0; col < maxCols; col++) {
                    int index = seekItem(row, col);

                    if (cellTag != null) {
                        Output.Write(cellTag.ToString(TagRenderMode.StartTag));
                    }

                    if (index < itemsCount) {
                        Output.Write(Display(items[index]));
                    }
                    else {
                        Output.Write(EmptyCell);
                    }

                    if (cellTag != null) {
                        Output.Write(cellTag.ToString(TagRenderMode.EndTag));
                    }
                }

                if (rowTag != null) {
                    Output.Write(rowTag.ToString(TagRenderMode.EndTag));
                }
            }

            if (gridTag != null) {
                Output.Write(gridTag.ToString(TagRenderMode.EndTag));
            }

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
