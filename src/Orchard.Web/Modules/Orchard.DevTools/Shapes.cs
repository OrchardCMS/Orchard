using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;

namespace Orchard.DevTools {
    public class Shapes : IShapeProvider {
        public IHtmlString Title(dynamic text) {
            return new HtmlString("<h2>" + text + "</h2>");
        }

        public IHtmlString Explosion(int Height, int Width) {
            return new HtmlString(string.Format("<div>Boom {0}x{1}</div>", Height, Width));
        }

        public IHtmlString Page(dynamic Display, dynamic Shape) {
            return Display(Shape.Sidebar, Shape.Messages);
        }

        public IHtmlString Zone(dynamic Display, dynamic Shape) {
            var tag = new TagBuilder("div");
            tag.GenerateId("zone-" + Shape.Name);
            tag.AddCssClass("zone-" + Shape.Name);
            tag.AddCssClass("zone");
            tag.InnerHtml = Smash(DisplayAll(Display, Shape)).ToString();
            return new HtmlString(tag.ToString());
        }

        public IHtmlString Message(dynamic Display, object Content, string Severity) {
            return Display(Severity, ": ", Content);
        }

        private static IHtmlString Smash(IEnumerable<IHtmlString> contents) {
            return new HtmlString(contents.Aggregate("", (a, b) => a + b));
        }

        static IEnumerable<IHtmlString> DisplayAll(dynamic Display, dynamic Shape) {
            foreach (var item in Shape) {
                yield return Display(item);
            }
        }
    }
}
