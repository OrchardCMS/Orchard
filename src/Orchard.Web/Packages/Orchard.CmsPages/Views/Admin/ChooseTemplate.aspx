<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.ViewModels.ChooseTemplateViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <div class="yui-u">
        <h2>Change Template</h2>
        <p class="bottomSpacer">
            Select your layout from one of the templates below.</p>
    </div>
    
    <div class="yui-u">
        <%using (Html.BeginForm()) { %>
        <%= Html.ValidationSummary() %>
        

<div class="yui-u first">

<ol>
        <% foreach (TemplateDescriptor template in Model.Templates) {%>
            <li>
            <ul>
            <li class="clearLayout">
            <h3><%=Html.Encode(template.DisplayName)%></h3>
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
        <li class="clearLayout"><input class="button" type="submit" value="Save Template Change" /><%=Html.ActionLink("Cancel", "Edit", new { Id = ViewContext.RouteData.GetRequiredString("id") }, new { @class = "linkButton" })%></li>
        </ol>
</div>

        <%}/*EndForm*/%>
    </div>
<% Html.Include("Foot"); %>