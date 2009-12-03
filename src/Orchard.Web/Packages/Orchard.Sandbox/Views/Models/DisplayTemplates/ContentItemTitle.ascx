<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IContent>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<%@ Import Namespace="Orchard.Models.ViewModels" %>
<%@ Import Namespace="Orchard.Models" %>
<h1><%=Html.ItemDisplayLink(Model) %></h1>
