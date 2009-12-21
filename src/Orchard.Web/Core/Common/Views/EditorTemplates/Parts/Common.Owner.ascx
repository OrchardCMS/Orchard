<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<OwnerEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels" %>
<%@ Import Namespace="Orchard.Core.Common.Models" %>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels" %>
<%@ Import Namespace="Orchard.Utility" %>
<fieldset>
    <legend>System</legend>
    <%=Html.LabelFor(m=>m.Owner) %>
    <%=Html.EditorFor(m=>m.Owner) %>
    <%=Html.ValidationMessageFor(m=>m.Owner) %>
</fieldset>
