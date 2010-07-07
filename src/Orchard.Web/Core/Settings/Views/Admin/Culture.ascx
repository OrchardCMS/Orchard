<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SiteCulturesViewModel>" %>
<%@ Import Namespace="Orchard.Core.Settings.ViewModels" %><%
 Html.RegisterStyle("admin.css"); %>
<h1><%:Html.TitleForPage(T("Cultures").ToString()) %></h1>
<p class="breadcrumb"><%:Html.ActionLink(T("Manage Settings").Text, "index") %><%:T(" &#62; ") %><%:T("Supported Cultures")%></p>
<h3><%:T("Available Cultures") %></h3>
<% using (Html.BeginFormAntiForgeryPost("AddCulture")) { %>
<%:Html.ValidationSummary() %>
<fieldset class="addCulture">
    <label for="CultureName"><%:T("Add a culture...") %></label>
    <%:Html.DropDownList("CultureName", new SelectList(Model.AvailableSystemCultures.OrderBy(s => s), Model.CurrentCulture))%>
    <button class="primaryAction" type="submit"><%:T("Add") %></button>
</fieldset>
<% } %>
<h3><%:T("Cultures this site supports") %></h3>
<%: Html.UnorderedList(
    Model.SiteCultures.OrderBy(s => s),
    (s, i) => Html.DisplayFor(scvm => s, s == Model.CurrentCulture ? "CurrentCulture" : "RemovableCulture", ""),
    "site-cultures", "culture", "odd")%>