<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<AddFieldViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %><%
Html.RegisterStyle("admin.css"); %>
<h1><%:Html.TitleForPage(T("Add a new field to {0}", Model.Part.Name).ToString())%></h1><%
using (Html.BeginFormAntiForgeryPost()) { %>
    <%:Html.ValidationSummary() %>
    <fieldset>
        <button class="primaryAction" type="submit"><%:T("Save") %></button>
    </fieldset><%
} %>
