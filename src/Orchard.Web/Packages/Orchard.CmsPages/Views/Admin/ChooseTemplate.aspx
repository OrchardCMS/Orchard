<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.ViewModels.ChooseTemplateViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.Services.Templates"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <h2>Change Template</h2>
    <p>Select your layout from one of the templates below.</p>    
    <% using (Html.BeginForm()) {
        %><%= Html.ValidationSummary() %>
        <fieldset>
            <ol class="templates">
                <% foreach (var template in Model.Templates) {
                      var t = template; %>
                    <li>
                        <%=Html.EditorFor(m => t) %>
                    </li>
                <% } %>
            </ol>
        </fieldset>
        <div>
            <input class="button" type="submit" value="Save Template Change" />
            <%=Html.ActionLink("Cancel", "Edit", new { Id = ViewContext.RouteData.GetRequiredString("id") }, new { @class = "cancel" })%>
        </div><%
       } %>
<% Html.Include("Foot"); %>