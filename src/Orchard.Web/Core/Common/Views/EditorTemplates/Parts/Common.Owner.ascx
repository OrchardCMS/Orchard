<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<OwnerEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<fieldset>
    <%: Html.LabelFor(m=>m.Owner) %>
    <%: Html.EditorFor(m=>m.Owner) %>
    <%: Html.ValidationMessageFor(m=>m.Owner) %>
</fieldset>