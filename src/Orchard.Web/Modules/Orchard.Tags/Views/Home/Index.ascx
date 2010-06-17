<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<TagsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h1 class="page-title"><%: Html.TitleForPage(T("Tags").ToString())%></h1>
<%: Html.UnorderedList(
    Model.Tags,
    (t, i) => Html.ActionLink(
        Html.Encode(t.TagName),
        "Search",
        new { tagName = t.TagName },
        new { @class = "" /* todo: (heskew) classify according to tag use */ }
        ).ToHtmlString(),
    "tagCloud")
     %>