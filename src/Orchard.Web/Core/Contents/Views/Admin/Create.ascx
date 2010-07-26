<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<CreateItemViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<h1><%:Html.TitleForPage((string.IsNullOrWhiteSpace(Model.Content.Item.TypeDefinition.DisplayName) ? T("Create Content") : T("Create {0}", Model.Content.Item.TypeDefinition.DisplayName)).ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
<%:Html.ValidationSummary() %>
<%:Html.EditorForItem(m=>m.Content) %>
<%} %>