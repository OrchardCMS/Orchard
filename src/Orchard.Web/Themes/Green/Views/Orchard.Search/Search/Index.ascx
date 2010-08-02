<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Search.ViewModels.SearchViewModel>" %>
<% Html.RegisterStyle("search.css"); %>
<h1 class="page-title"><%:Html.TitleForPage(T("Search").Text)%></h1><%
if (!string.IsNullOrWhiteSpace(Model.Query)) {
    if (Model.PageOfResults.Count() == 0) { %>
    <p class="search-summary"><%=T.Plural("There is <em>one</em> result", "Sorry, there are <em>no</em> results", Model.PageOfResults.Count()) %></p><%
    }
    else { %>
    <p class="search-summary"><%=T.Plural("There is <em>one</em> result", "There are <em>{0}</em> results", Model.PageOfResults.TotalItemCount, Model.PageOfResults.StartPosition, Model.PageOfResults.EndPosition)%></p><%
    }
}
if (Model.PageOfResults != null && Model.PageOfResults.Count() > 0) { %>
<%=Html.UnorderedList(Model.PageOfResults, (r, i) => Html.DisplayForItem(r.Content) , "search-results contentItems") %>
<%=Html.Pager(Model.PageOfResults, Model.PageOfResults.PageNumber, Model.DefaultPageSize, new {q = Model.Query}) %><%
} %>