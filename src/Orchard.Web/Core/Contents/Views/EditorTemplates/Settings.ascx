<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentManagement.MetaData.Models.SettingsDictionary>" %><%
if (Model.Any()) { %>
    <fieldset><%
        var si = 0;
        foreach (var setting in Model) {
            var s = setting;
            var htmlFieldName = string.Format("Settings[{0}]", si++); %>
            <%--// doesn't gen a similarly sanitized id as the other inputs...--%>
            <label for="<%:ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName + ".Value") %>"><%:s.Key %></label>
            <%:Html.Hidden(htmlFieldName + ".Key", s.Key) %>
            <%:Html.TextBox(htmlFieldName + ".Value", s.Value)%><%
        } %>
    </fieldset><%
} %>