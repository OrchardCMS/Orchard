<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TemplateDescriptor>" %>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates" %>
<div>
    <h3><%=Html.Encode(Model.DisplayName)%></h3>
    <fieldset>
        <label title="<%=Html.Encode(Model.DisplayName)%>" for="TemplateName_<%=Model.Name%>">
        <%-- todo: (heskew) need to know if this template descriptor is what's currently set. view model? --%>
        <input type="radio" name="TemplateName" checked="<%="" == Model.Name ? "checked" : ""%>" value="<%=Model.Name %>" id="<%=string.Format("TemplateName_{0}", Model.Name) %>" />
        Select this template</label>
    </fieldset>
    <%-- todo: (heskew) need an HTML helper (+route and controller/action for an ImageResult) for template thumbnails --%>
    <p><img src="/Packages/Orchard.CMSPages/Views/Templates/<%=Html.Encode(Model.Name)%>.gif" alt="<%=Html.Encode(Model.DisplayName)%>" /><%=Html.Encode(Model.Description)%></p>
</div>