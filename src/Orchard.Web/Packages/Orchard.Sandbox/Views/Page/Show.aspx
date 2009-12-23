<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageShowViewModel>" %>

<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<%= Html.DisplayForItem(Model.Page) %>
