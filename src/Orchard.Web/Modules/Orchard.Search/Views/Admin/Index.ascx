<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Search.ViewModels.SearchIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %><%
Html.RegisterStyle("admin.css"); %>
<h1><%=Html.TitleForPage(T("Search Index Mangement").ToString()) %></h1><%
using (Html.BeginForm("update", "admin", FormMethod.Post, new {area = "Orchard.Search"})) { %>
    <fieldset>
        <p><%=T("The search index was last updated {0}. <button type=\"submit\" title=\"Update the search index.\" class=\"primaryAction\">Update</button>", Html.DateTimeRelative(Model.IndexUpdatedUtc))%></p>
        <%=Html.AntiForgeryTokenOrchard() %>
    </fieldset><%
}
using (Html.BeginForm("rebuild", "admin", FormMethod.Post, new {area = "Orchard.Search"})) { %>
    <fieldset>
        <p><%=T("Rebuild the search index for a fresh start. <button type=\"submit\" title=\"Rebuild the search index.\">Rebuild</button>") %></p>
        <%=Html.AntiForgeryTokenOrchard() %>
    </fieldset><%
} %>