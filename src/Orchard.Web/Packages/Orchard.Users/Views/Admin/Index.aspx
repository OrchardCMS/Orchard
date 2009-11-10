<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Users.ViewModels.UsersIndexViewModel>" %>

<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Manage Users</title>
    <% Html.Include("Head"); %>
</head>
<body>
    <% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <div class="yui-g">
        <h2>
            Manage Users</h2>
        
        <%=Html.ValidationSummary() %>
        <%=Html.ActionLink("Add a new user", "Create", new {}, new {@class="floatRight topSpacer"}) %>
        <table id="pluginListTable" cellspacing="0" class="clearLayout">
            <colgroup>
                <col id="Name" />
                <col id="Email" />
                <col id="Edit" />
            </colgroup>
            <thead>
                <tr>
                    <th scope="col">
                        Name
                    </th>
                    <th scope="col">
                        Email
                    </th>
                    <th scope="col">
                    </th>
                </tr>
            </thead>
            <% foreach (var row in Model.Rows) { %>
            <tr>
                <td>
                    <%=Html.Encode(row.User.As<IUser>().UserName) %>
                </td>
                <td>
                    <%=Html.Encode(row.User.As<IUser>().Email) %>
                </td>
                <td>
                    <%=Html.ActionLink("Edit", "Edit", new { row.User.Id })%>
                </td>
            </tr>
            <%}%>
        </table>
    </div>
    <% Html.EndForm(); %>
    <% Html.Include("Footer"); %>
</body>
</html>
