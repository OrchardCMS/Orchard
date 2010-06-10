<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContainerEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels" %>
<fieldset>
    <%=Html.LabelFor(m=>m.ContainerId) %>
    <%=Html.EditorFor(m=>m.ContainerId) %>
    <%=Html.ValidationMessageFor(m=>m.ContainerId) %>
</fieldset>
