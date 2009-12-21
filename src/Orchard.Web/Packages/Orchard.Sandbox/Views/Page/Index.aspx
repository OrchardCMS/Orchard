<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageIndexViewModel>" %>

<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h3>
    Sandbox Pages</h3>
<p>
    <%=Html.ActionLink("Create new page", "create") %></p>
<%foreach (var item in Model.Pages) {%>
<%=Html.DisplayForItem(item) %>
<%}%>
