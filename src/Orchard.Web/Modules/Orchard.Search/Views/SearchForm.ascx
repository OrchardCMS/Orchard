<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SearchViewModel>" %>
<%@ Import Namespace="Orchard.Search.ViewModels" %><%
using(Html.BeginForm("index", "search", new { area = "Orchard.Search" }, FormMethod.Get, new { @class = "search" })) { %>
    <fieldset>
        <%=Html.TextBox("q", Model.Term) %>
        <button type="submit"><%=T("Search") %></button>
    </fieldset><%
} %>