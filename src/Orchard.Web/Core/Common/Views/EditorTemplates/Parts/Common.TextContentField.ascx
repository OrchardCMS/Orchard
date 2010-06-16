<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<TextContentFieldEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<fieldset>
    <%: Html.LabelFor(m=>m.Text) %>
    <%: Html.EditorFor(m=>m.Text) %>
    <%: Html.ValidationMessageFor(m=>m.Text) %>
</fieldset>