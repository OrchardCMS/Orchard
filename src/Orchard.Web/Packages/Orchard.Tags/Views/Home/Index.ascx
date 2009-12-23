<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TagsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h2><%=Html.TitleForPage("Tags")%></h2>
<%=Html.UnorderedList(
    Model.Tags,
    (t, i) => Html.ActionLink(
        t.TagName,
        "Search",
        new { tagName = t.TagName },
        new { @class = "" /* todo: (heskew) classify according to tag use */ }
        ).ToHtmlString(),
    "tagCloud")
     %>