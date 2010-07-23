<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Packaging.ViewModels.PackagingSourcesViewModel>" %>
<h1>
    <%: Html.TitleForPage(T("Manage Feeds").ToString())%></h1>

<ul>
    <%foreach (var item in Model.Sources) {%><li>
        <%:Html.Link(item.FeedUrl, item.FeedUrl)%></li>
    <% }%>
</ul>

<%using (Html.BeginFormAntiForgeryPost(Url.Action("AddSource"))) {%>
Url: <%:Html.TextBox("url") %> <input type="submit" value="Add Source" />
<%} %>
