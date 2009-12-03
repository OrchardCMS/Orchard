<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemEditorViewModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Sandbox.Models" %>
<%@ Import Namespace="Orchard.Models.ViewModels" %>
<%@ Import Namespace="Orchard.Models" %>

                    
<li>
<%=Html.LabelFor(m => m.Item.Record.Name)%>
<%=Html.EditorFor(m => m.Item.Record.Name)%>
</li>

<%foreach (var e in Model.Editors) { %>
<%=Html.EditorFor(m=>e.Model, e.TemplateName, e.Prefix??"") %>
<%} %>
