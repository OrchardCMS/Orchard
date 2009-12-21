<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BodyEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels" %>
<%@ Import Namespace="Orchard.Core.Common.Models" %>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels" %>
<%@ Import Namespace="Orchard.Utility" %>
<fieldset>
    <label>Body</label>
    <%=Html.EditorFor(m=>m.Text, Model.TextEditorTemplate) %>
</fieldset>