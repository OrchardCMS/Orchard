<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PageViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Pages.ViewModels"%>
<%=Html.DisplayForItem(m=>m.Page) %>