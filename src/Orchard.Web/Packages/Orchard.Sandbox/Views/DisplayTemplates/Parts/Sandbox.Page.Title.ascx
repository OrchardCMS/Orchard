<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IContent>" %>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels" %>
<%@ Import Namespace="Orchard.ContentManagement" %>
<h1><%=Html.ItemDisplayLink(Model) %></h1>
