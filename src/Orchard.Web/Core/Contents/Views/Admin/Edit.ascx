<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<EditItemViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<h1><%:Html.TitleForPage((string.IsNullOrEmpty(Model.Content.Item.TypeDefinition.DisplayName) ? T("Edit Content") : T("Edit {0}", Model.Content.Item.TypeDefinition.DisplayName)).ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
<%:Html.ValidationSummary() %>
<%:Html.EditorForItem(m=>m.Content) %>
<%} %>
