<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<EditPartFieldViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
    <dt><%:Model.Name %> <span>(<%:Model.FieldDefinition.Name %>)</span></dt>
    <dd>
        <%:Html.DisplayFor(m => m.Settings, "Settings", "") %>
    </dd>