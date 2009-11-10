<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Users.ViewModels.UserCreateViewModel>" %>

<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Add a new User</title>
    <% Html.Include("Head"); %>
</head>
<body>
    <% Html.Include("Header"); %>
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
    <% Html.Include("Footer"); %>
</body>
</html>
