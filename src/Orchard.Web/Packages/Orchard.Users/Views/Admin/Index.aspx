<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Users.ViewModels.UsersIndexViewModel>" %>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Security" %>
<h2><%=Html.TitleForPage("Manage Users") %></h2>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary()%>
    <div class="manage"><%=Html.ActionLink("Add a new user", "Create", new { }, new { @class = "button" })%></div>
    <fieldset>
        <table class="items">
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
            <% foreach (var row in Model.Rows)
               { %>
            <tr>
                <td>
                    <%=Html.Encode(row.User.UserName)%>
                </td>
                <td>
                    <%=Html.Encode(row.User.Email)%>
                </td>
                <td>
                    <%=Html.ActionLink("Edit", "Edit", new { row.User.Id })%>
                </td>
            </tr>
            <%}%>
        </table>
    </fieldset>
<% } %>