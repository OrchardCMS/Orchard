using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;

namespace Orchard.DevTools {
    public class Shapes : IShapeDriver {
        public IHtmlString Title(dynamic text) {
            return new HtmlString("<h2>" + text + "</h2>");
        }

        public IHtmlString Explosion(int? Height, int? Width) {
            return new HtmlString(string.Format("<span>Boom {0}x{1}</span>", Height, Width));
        }

        public IHtmlString Page(dynamic Display, dynamic Shape) {
            return Display(Shape.Sidebar, Shape.Messages);
        }

        public IHtmlString Zone(dynamic Display, dynamic Shape) {
            var tag = new TagBuilder("div");
            tag.GenerateId("zone-" + Shape.Name);
            tag.AddCssClass("zone-" + Shape.Name);
            tag.AddCssClass("zone");
            tag.InnerHtml = Combine(DisplayAll(Display, Shape).ToArray()).ToString();
            return new HtmlString(tag.ToString());
        }
        
        public IHtmlString Message(dynamic Display, object Content, string Severity) {
            return Display(new HtmlString("<p class=\"message\">"), Severity ?? "Neutral", ": ", Content, new HtmlString("</p>"));
        }


        static IHtmlString Combine(IEnumerable<IHtmlString> contents) {
            return new HtmlString(contents.Aggregate("", (a, b) => a + b));
        }

        static IEnumerable<IHtmlString> DisplayAll(dynamic Display, dynamic Shape) {
            foreach (var item in Shape) {
                yield return Display(item);
            }
        }
    }
}
