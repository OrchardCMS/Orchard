<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Search.ViewModels.SearchViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %><%
Html.RegisterStyle("search.css"); %>
<h1><%=Html.TitleForPage(T("Search").Text)%></h1><%
Html.Zone("search");
if (!string.IsNullOrWhiteSpace(Model.Query)) {
    if (Model.Count == 0) { %>
    <p class="search-summary"><%=T("<em>zero</em> results") %></p><%
    }
    else { %>
    <p class="search-summary"><%=T("<em>{0} - {1}</em> of <em>{2}</em> results", (Model.Page - 1) * Model.PageSize + 1, Model.Page * Model.PageSize > Model.Count ? Model.Count : Model.Page * Model.PageSize, Model.Count)%></p><%
    }
}
if (Model.ResultsPage != null && Model.ResultsPage.Count() > 0) { %>
<%=Html.UnorderedList(Model.ResultsPage, (r, i) => Html.DisplayForItem(r.Content).ToHtmlString(), "search-results contentItems") %>
<%=Html.Pager(Model.TotalPageCount, Model.Page, new {q = Model.Query}) %><%
} %>