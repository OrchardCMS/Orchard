<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Search.ViewModels.SearchViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %><%
Html.RegisterStyle("search.css"); %>
<h1><%=Html.TitleForPage(T("Search"))%></h1><%
using(Html.BeginForm("index", "search", FormMethod.Get, new { @class = "search" })) { %>
    <fieldset>
        <%=Html.TextBox("q", Model.Term) %>
        <button type="submit"><%=T("Search") %></button>
    </fieldset><%
}

if (Model.Results.Count() > 0) { %>
    <p class="search-summary"><%=T("<em>{0}</em> results", Model.Results.Count()) %></p>
    <%=Html.UnorderedList(Model.Results, (r, i) => Html.DisplayForItem(r.Content).ToHtmlString(), "search-results contentItems") %><%
} %>