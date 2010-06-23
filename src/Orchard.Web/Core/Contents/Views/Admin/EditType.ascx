<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<EditTypeViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %><%
Html.RegisterStyle("admin.css"); %>
<h1><%:Html.TitleForPage(T("Edit Content Type").ToString())%></h1><%
using (Html.BeginFormAntiForgeryPost()) { %>
    <%:Html.ValidationSummary() %>
    <fieldset>
        <label for="DisplayName"><%:T("Display Name") %></label>
        <%:Html.TextBoxFor(m => m.DisplayName, new {@class = "textMedium"}) %>
        <%--// has unintended consequences (renamging the type) - changing the name creates a new type of that name--%>
        <label for="Name"><%:T("Name") %></label>
        <%:Html.TextBoxFor(m => m.Name, new {@class = "textMedium", disabled = "disabled"}) %>
        <%:Html.HiddenFor(m => m.Name) %>
    </fieldset>
    <%:Html.EditorFor(m => m.Settings) %>
    <h2><%:T("Parts") %></h2>
    <div class="manage add-to-type"><%: Html.ActionLink(T("Add").Text, "AddPart", new { }, new { @class = "button" })%></div>
    <%:Html.EditorFor(m => m.Parts, "Parts", "") %>
    <h2><%:T("Fields") %></h2>
    <div class="manage add-to-type"><%: Html.ActionLink(T("Add").Text, "AddField", new { }, new { @class = "button" })%></div>
    <%:Html.EditorFor(m => m.Fields, "Fields", "")%>
    <fieldset>
        <button class="primaryAction" type="submit"><%:T("Save") %></button>
    </fieldset><%
} %>
