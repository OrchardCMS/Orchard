<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ShellSettings>" %>
<%@ Import Namespace="Orchard.Environment.Configuration" %>
<%=Html.ActionLink(T("Suspend").ToString(), "_disable", new {tenantName = Model.Name, area = "Orchard.MultiTenancy"}) %>