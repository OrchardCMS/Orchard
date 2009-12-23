<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageIndexViewModel>" %>
<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<h1><%=Html.TitleForPage("Sandbox Pages")%></h1>
<p><%=Html.ActionLink("Create new page", "create") %></p>
<%foreach (var item in Model.Pages) {%>
    <%=Html.DisplayForItem(item) %>
<%}%>
