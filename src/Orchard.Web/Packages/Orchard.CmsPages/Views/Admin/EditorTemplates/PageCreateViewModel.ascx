<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.CmsPages.ViewModels.PageCreateViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates" %>
<%@ Import Namespace="Orchard.CmsPages.ViewModels" %>
<%@ Import Namespace="Orchard.Utility" %>
<%--
<ol>
    <%=Html.EditorFor(m=>m.Title, "inputTextLarge") %>
    <%=Html.EditorFor(m=>m.Slug, "inputTextLarge") %>
</ol>--%>

<div class="yui-u first">
        <ol>
        <% foreach (TemplateDescriptor template in Model.Templates) {%>
            <li>
            <ul>
            <li class="clearLayout">
            <h4 class="separator"><%=Html.Encode(template.DisplayName)%></h4>
            </li>
            <li>
            <label title="<%=Html.Encode(template.DisplayName)%>" for="TemplateName_<%=template.Name%>">
            <%=Html.RadioButton("TemplateName", template.Name, Model.TemplateName == template.Name, new{id="TemplateName_" + template.Name})%>
            Select this template</label>
            </li>
            <li class="floatLeft"><img src="<%=Html.Encode(template.Name)%>.gif" alt="<%=Html.Encode(template.DisplayName)%>" class="templateThumbnail" /></li>
            <li class="floatLeft"><p><%=Html.Encode(template.Description)%></p></li>
            </ul>
            </li>
        <% } %>
        <li class="clearLayout"><input class="button" type="submit" value="Create" /></li>
        </ol>

</div>
