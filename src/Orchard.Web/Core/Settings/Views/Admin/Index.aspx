<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Core.Settings.ViewModels.SettingsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("Header"); %>
    <div class="yui-u">
        <h2 class="separator">
            Edit Settings</h2>
    </div>
    <div class="yui-u">
        <%using (Html.BeginForm()) { %>
        <ol>
            <%= Html.ValidationSummary() %>
            <%= Html.EditorForModel() %>
            <li class="clearLayout">
            <%--<label for="SiteName">Site Name:</label>
                <input id="SiteName" class="inputText inputTextLarge roundCorners" name="SiteName" type="text" value="<%= Model.SiteSettings.SiteName %>" />    
            <label for="SuperUser">Super User Name:</label>    
                <input id="SuperUser" class="inputText inputTextLarge roundCorners" name="SuperUser" type="text" value="<%= Model.SiteSettings.SuperUser %>" />
--%>                <input class="button" type="submit" value="Save" /> 
                <%=Html.ActionLink("Cancel", "Index", new{}, new{@class="button"}) %>
                </li>
        </ol>
        <%}/*EndForm*/%>
    </div>
<% Html.Include("Footer"); %>