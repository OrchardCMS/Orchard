<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<EditItemViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<h1><%:Html.TitleForPage(T("Translate Content").ToString())%></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
<%:Html.ValidationSummary() %>
<%:Html.EditorForItem(m=>m.Content) %>
<%} %>
