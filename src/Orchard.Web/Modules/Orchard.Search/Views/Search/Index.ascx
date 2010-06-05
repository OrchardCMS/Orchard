<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Search.ViewModels.SearchViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %><%
Html.RegisterStyle("search.css"); %>
<h1><%=Html.TitleForPage(T("Search").Text)%></h1><%
Html.Zone("search");
if (!string.IsNullOrWhiteSpace(Model.Query)) { %>
<p class="search-summary"><%=T("<em>{0}</em> results", Model.Results.Count()) %></p><%
}
if (Model.Results.Count() > 0) { %>
<%=Html.UnorderedList(Model.Results, (r, i) => Html.DisplayForItem(r.Content).ToHtmlString(), "search-results contentItems") %><%
} %>