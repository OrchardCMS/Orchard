<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<AddLocalizationViewModel>" %>
<%@ Import Namespace="Orchard.Core.Localization.ViewModels" %><%
Model.Content.Zones.AddRenderPartial("primary:before", "CultureSelection", Model); %>
<h1><%:Html.TitleForPage(T("Translate Content").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
<%:Html.ValidationSummary() %>
<%:Html.EditorForItem(m=>m.Content) %>
<%} %>