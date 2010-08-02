<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ContentItemViewModel<SandboxPagePart>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Sandbox.Models"%>
<fieldset>
    <%: Html.LabelFor(m => m.Item.Record.Name) %>
    <%: Html.EditorFor(m => m.Item.Record.Name) %>
</fieldset>
<% Html.ZonesAny(); %>