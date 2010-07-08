<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Modules.Packaging.ViewModels.PackagingSourcesViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h1>
    <%: Html.TitleForPage(T("Packaging").ToString(), T("Edit Sources").ToString())%></h1>
    <%: Html.Partial("_Subnav") %>

<ul>
    <%foreach (var item in Model.Sources) {%><li>
        <%:Html.Link(item.FeedUrl, item.FeedUrl)%></li><%
                                   }%></ul>
<%using (Html.BeginFormAntiForgeryPost(Url.Action("AddSource"))) {%>
Url:
<%:Html.TextBox("url") %>
<input type="submit" value="Add Source" />
<%} %>
