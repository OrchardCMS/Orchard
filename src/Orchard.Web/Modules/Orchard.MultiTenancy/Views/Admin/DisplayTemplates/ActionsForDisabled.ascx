<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ShellSettings>" %>
<%@ Import Namespace="Orchard.Environment.Configuration" %>
<%=Html.ActionLink(T("Resume").ToString(), "_enable", new {tenantName = Model.Name, area = "Orchard.MultiTenancy"}) %>