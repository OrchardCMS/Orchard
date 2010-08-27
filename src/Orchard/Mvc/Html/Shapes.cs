using System.Web;
using System.Web.Mvc;
using ClaySharp;
using Orchard.DisplayManagement;

namespace Orchard.Mvc.Html {
    public class Shapes : IShapeDriver {
        public IHtmlString Link(dynamic Display, object Content, string Url, INamedEnumerable<object> Attributes) {
            var tagBuilder = new TagBuilder("a") { InnerHtml = Display(Content).ToString() };
            tagBuilder.MergeAttributes(Attributes.Named);
            tagBuilder.MergeAttribute("href", Url);
            return Display(new HtmlString(tagBuilder.ToString(TagRenderMode.Normal)));
        }

        public IHtmlString Image(dynamic Display, string Url, INamedEnumerable<object> Attributes) {
            var tagBuilder = new TagBuilder("img");
            tagBuilder.MergeAttributes(Attributes.Named);
            tagBuilder.MergeAttribute("src", Url);
            return Display(new HtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing)));
        }

        #region form and company

        public IHtmlString Form(dynamic Display, dynamic Shape, object Submit) {
            return Display(new HtmlString("<form>" + Display(Shape.Fieldsets, Submit).ToString() + "</form>"));
        }

        public IHtmlString FormSubmit(dynamic Display, dynamic Shape) {
            return Display(new HtmlString("<button type='submit'>" + Shape.Text + "</button>"));
        }

        public IHtmlString InputPassword(dynamic Display, dynamic Shape) {
            return Display(new HtmlString("<label>" + Shape.Text + "</label><input type='password' name='" + Shape.Name + "' value='" + (Shape.Value == null ? "" : Shape.Value) + "' />"));
        }

        public IHtmlString InputText(dynamic Display, dynamic Shape, INamedEnumerable<object> Attributes) {
            // not optimal obviously, just testing the waters
            return Display(new HtmlString("<label>" + Shape.Text + "</label>"
                + "<input type='text'" + (Attributes.Named.ContainsKey("autofocus") ? " autofocus" : "" )+ " name='" + Shape.Name
                    + "' value='" + (Shape.Value == null ? "" : Shape.Value) + "' />")); // <- otherwise Shape.Value is "ClaySharp.Clay"
        }

        #endregion
    }
}
