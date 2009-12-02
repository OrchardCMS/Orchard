<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Users.ViewModels.UserEditViewModel>" %>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("AdminHead"); %>
    <div class="yui-u">
        <h2 class="separator">
            Edit User</h2>
        <p class="bottomSpacer">
        <%=Html.ActionLink("Users", "Index") %> > Edit #<%= Model.Id%> <strong><%=Html.Encode(Model.UserName)%></strong>
        </p>
    </div>
    <div class="yui-u">
        <%using (Html.BeginForm()) { %>
        <ol>
            <%= Html.ValidationSummary() %>
            <%= Html.EditorForModel() %>
            <li class="clearLayout">
                <input class="button" type="submit" value="Save" /> 
                <%=Html.ActionLink("Cancel", "Index", new{}, new{@class="button"}) %>
                </li>
        </ol>
        <%}/*EndForm*/%>
    </div>
<% Html.Include("AdminFoot"); %>