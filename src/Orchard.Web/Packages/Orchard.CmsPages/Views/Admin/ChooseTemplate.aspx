<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.ViewModels.ChooseTemplateViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Change Template</h2>
<p>Select your layout from one of the templates below.</p>    
<% using (Html.BeginForm()) {
    %><%= Html.ValidationSummary() %>
    <ul class="templates"><%
        foreach (var template in Model.Templates) {
            var t = template; %>
        <li><%=
            Html.EditorFor(m => t) %>
        </li><%
        } %>
    </ul>
    <p>
        <input class="button" type="submit" value="Save Template Change" />
        <%-- todo: (heskew) should pull to give the browser some chance of rehydrating the edit page form state --%>
        <%=Html.ActionLink("Cancel", "Edit", new { Id = ViewContext.RouteData.GetRequiredString("id") }, new { @class = "cancel" })%>
    </p><%
   } %>