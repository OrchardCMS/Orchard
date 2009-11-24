<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TemplateDescriptor>" %>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates" %>
<ul>
    <li>
        <h3><%=Html.Encode(Model.DisplayName)%></h3>
    </li>
    <li>
        <label title="<%=Html.Encode(Model.DisplayName)%>" for="TemplateName_<%=Model.Name%>">
        <%-- todo: (heskew) need to know if this template descriptor is what's currently set. view model? --%>
        <input type="radio" name="TemplateName" checked="<%="" == Model.Name ? "checked" : ""%>" value="<%=Model.Name %>" id="<%=string.Format("TemplateName_{0}", Model.Name) %>" />
        Select this template</label>
    </li>
    <li>
        <p><img src="<%=Html.Encode(Model.Name)%>.gif" alt="<%=Html.Encode(Model.DisplayName)%>" /><%=Html.Encode(Model.Description)%></p>
    </li>
</ul>