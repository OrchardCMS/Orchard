<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemEditorModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<%@ Import Namespace="Orchard.Models.ViewModels" %>

<li>
    <%=Html.LabelFor(m => m.Item.Record.Name)%>
    <%=Html.EditorFor(m => m.Item.Record.Name)%>
</li>
<%=Html.EditorZonesAny()%>
