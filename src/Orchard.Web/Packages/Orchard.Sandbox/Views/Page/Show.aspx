<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageShowViewModel>" %>

<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%= Html.DisplayForItem(Model.Page) %>
