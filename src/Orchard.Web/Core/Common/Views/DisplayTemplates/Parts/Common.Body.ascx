<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BodyDisplayViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels"%>
<%@ Import Namespace="Orchard.Utility" %>

<%= Model.BodyAspect.Record.Text %>
