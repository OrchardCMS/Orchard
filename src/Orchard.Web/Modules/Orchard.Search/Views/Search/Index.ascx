<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Search.ViewModels.SearchViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %><%
Html.RegisterStyle("search.css"); %>
<h1><%: Html.TitleForPage(T("Search").Text)%></h1><%
Html.Zone("search");
if (!string.IsNullOrWhiteSpace(Model.Query)) {
    if (Model.PageOfResults.Count() == 0) { %>
    <p class="search-summary"><%: T("<em>zero</em> results") %></p><%
    }
    else { %>
    <p class="search-summary"><%: T("<em>{0} - {1}</em> of <em>{2}</em> results", Model.PageOfResults.StartPosition, Model.PageOfResults.EndPosition, Model.PageOfResults.TotalItemCount)%></p><%
    }
}
if (Model.PageOfResults != null && Model.PageOfResults.Count() > 0) { %>
<%: Html.UnorderedList(Model.PageOfResults, (r, i) => Html.DisplayForItem(r.Content).ToHtmlString() , "search-results contentItems") %>
<%: Html.Pager(Model.PageOfResults, Model.PageOfResults.PageNumber, Model.DefaultPageSize, new {q = Model.Query}) %><%
} %>