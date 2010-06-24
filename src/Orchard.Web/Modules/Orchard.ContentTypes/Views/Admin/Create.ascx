<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentTypes.ViewModels.CreateTypeViewModel>" %>

<h1><%:Html.TitleForPage(T("New Content Type").ToString())%></h1><%
using (Html.BeginFormAntiForgeryPost()) { %>
    <%:Html.ValidationSummary() %>
    <fieldset>
        <label for="DisplayName"><%:T("Display Name") %></label>
        <%:Html.TextBoxFor(m => m.DisplayName, new {@class = "textMedium", autofocus = "autofocus"}) %>
    </fieldset>
    <fieldset>
        <button class="primaryAction" type="submit"><%:T("Create") %></button>
    </fieldset><%
} %>