<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemViewModel<SandboxPage>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Sandbox.Models" %>

<li>
    <%=Html.LabelFor(m => m.Item.Record.Name)%>
    <%=Html.EditorFor(m => m.Item.Record.Name)%>
</li>
<% Html.ZonesAny(); %>
