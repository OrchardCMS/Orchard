<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentTypes.ViewModels.AddFieldViewModel>" %>
<%
Html.RegisterStyle("admin.css"); %>
<h1><%:Html.TitleForPage(T("Add a new field to {0}", Model.Part.Name).ToString())%></h1><%
using (Html.BeginFormAntiForgeryPost()) { %>
    <%:Html.ValidationSummary() %>
    <fieldset>
        <label for="DisplayName"><%:T("Display Name") %></label>
        <%:Html.TextBoxFor(m => m.DisplayName, new {@class = "textMedium", autofocus = "autofocus"}) %>
    </fieldset>
    <fieldset>
        <label for="FieldTypeName"><%:T("Field Type") %></label>
        <%:Html.DropDownListFor(m => m.FieldTypeName, new SelectList(Model.Fields, "FieldTypeName", "FieldTypeName"))%>
    </fieldset>
    <fieldset>
        <button class="primaryAction" type="submit"><%:T("Save") %></button>
    </fieldset><%
} %>
