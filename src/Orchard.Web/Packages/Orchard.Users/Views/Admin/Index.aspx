<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<UsersIndexViewModel>" %>
<%@ Import Namespace="Orchard.Users.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Users").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary()%>
    <div class="manage"><%=Html.ActionLink(T("Add a new user").ToString(), "Create", new { }, new { @class = "button" })%></div>
    <fieldset>
        <table class="items">
            <colgroup>
                <col id="Name" />
                <col id="Email" />
                <col id="Edit" />
            </colgroup>
            <thead>
                <tr>
                    <th scope="col"><%=_Encoded("Name")%></th>
                    <th scope="col"><%=_Encoded("Email")%></th>
                    <th scope="col"><%=_Encoded("") %></th>
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
                    <%=Html.ActionLink(T("Edit").ToString(), "Edit", new { row.User.Id })%> | 
                    <%=Html.ActionLink(T("Delete").ToString(), "Delete", new { row.User.Id })%> 
                </td>
            </tr>
            <%}%>
        </table>
    </fieldset>
<% } %>