using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Helpers {
    public static class EditorResultExtensions {
        public static IEnumerable<string> CollectTabs(this EditorResult editorResult) {
            var set = new HashSet<ShapePosition>();

            foreach (var editor in editorResult.Editors) {
                var positionText = editor.Metadata.Position;

                if (!String.IsNullOrWhiteSpace(positionText)) {
                    var position = ShapePosition.Parse(positionText);
                    set.Add(position);
                }
            }
            return set.Distinct(new ShapePositionDistinctComparer()).OrderBy(x => x.Position).Select(x => x.Name);
        }
    }
}