<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SiteCulturesViewModel>" %>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels" %><%
 Html.RegisterStyle("admin.css"); %>
<h1><%:Html.TitleForPage(T("Manage Settings").ToString()) %></h1>
<h2><%:T("Cultures this site supports") %></h2>
<%: Html.UnorderedList(
    Model.SiteCultures.OrderBy(s => s),
    (s, i) => Html.DisplayFor(scvm => s, s == Model.CurrentCulture ? "CurrentCulture" : "RemovableCulture", "").ToString(),
    "site-cultures", "culture", "odd")%>
<% using (Html.BeginFormAntiForgeryPost("AddCulture")) { %>
<%:Html.ValidationSummary() %>
<fieldset>
    <legend><%:T("Add a culture...") %></legend>
    <%:Html.DropDownList("CultureName", new SelectList(Model.AvailableSystemCultures.OrderBy(s => s), Model.CurrentCulture)) %>
    <button class="primaryAction" type="submit"><%:T("Add") %></button>
</fieldset>
<% } %>