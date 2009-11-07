<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PageRevision>" %>
<%@ Import Namespace="Orchard.CmsPages.Models"%>
<%
    if (Request.IsAuthenticated) {
%>
        [ <%=Html.ActionLink("Edit", "Edit", "Admin", new{Area="Orchard.CmsPages", Model.Page.Id}, new{})%> ]
<%
    }
%> 
