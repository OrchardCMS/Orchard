<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IContent>" %>
<%@ Import Namespace="Orchard.ContentManagement"%>
<h2><%=Html.ItemDisplayLink(Model) %></h2>