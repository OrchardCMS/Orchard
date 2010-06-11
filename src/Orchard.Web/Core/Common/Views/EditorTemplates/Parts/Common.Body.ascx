<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BodyEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels" %>
<fieldset>
    <label><%: T("Body")%></label>
    <%: Html.Partial("EditorTemplates/" + Model.TextEditorTemplate, Model) %>
    <%: Html.ValidationMessageFor(m => m.Text) %>
</fieldset>