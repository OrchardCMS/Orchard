<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PageRevision>" %>
<%@ Import Namespace="Orchard.Pages.Models"%>
<%
    if (Request.IsAuthenticated) {
%>
        [ <%=Html.ActionLink("Edit", "Edit", "Admin", new{Area="Orchard.Pages", Model.Page.Id}, new{})%> ]
<%
    }
%> 
