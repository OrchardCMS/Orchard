<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BodyEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels" %>
<%@ Import Namespace="Orchard.Core.Common.Models" %>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels" %>
<%@ Import Namespace="Orchard.Utility" %>
<h3>Body</h3>
<ul><li><%=Html.EditorFor(m=>m.Text, Model.TextEditorTemplate) %></li></ul>
