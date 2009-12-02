<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Users.ViewModels.UserCreateViewModel>" %>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
    <div class="yui-u">
        <h2 class="separator">
            Add a new User</h2>
            
    </div>
    <div class="yui-u">
        <%using (Html.BeginForm()) { %>
        <ol>
            <%= Html.ValidationSummary() %>
            <%= Html.EditorForModel() %>
            <li class="clearLayout">
                <input class="button" type="submit" value="Create" />
                <%=Html.ActionLink("Cancel", "Index", new{}, new{@class="button"}) %></li>
        </ol>
        <%}/*EndForm*/%>
    </div>
<% Html.Include("AdminFoot"); %>