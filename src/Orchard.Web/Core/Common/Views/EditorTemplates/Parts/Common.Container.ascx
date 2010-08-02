<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContainerEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels" %>
<fieldset>
    <%: Html.HiddenFor(m=>m.ContainerId) %>
</fieldset>