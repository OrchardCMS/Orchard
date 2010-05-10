<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ShellSettings>" %>
<%@ Import Namespace="Orchard.MultiTenancy.Extensions"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Environment.Configuration" %>
<%=Html.Link(T("Set Up").ToString(), Url.Tenant(Model))%>