<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Pages.ViewModels.ChooseTemplateViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Pages.Services.Templates"%>
<h2><%=Html.TitleForPage("Change Template") %></h2>
<p>Select your layout from one of the templates below.</p>    
<% using (Html.BeginFormAntiForgeryPost()) {
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