<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BodyEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels" %>
<fieldset>
    <label><%=_Encoded("Body")%></label>
    <%=Html.EditorFor(m => m.Text, Model.TextEditorTemplate) %>
    <%=Html.ValidationMessageFor(m => m.Text) %>
</fieldset>