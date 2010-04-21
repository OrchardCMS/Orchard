<%@ Page %>

<ul>
    <% foreach(string name in Request.Cookies) {%>
    <li>
        <%=name %>:<%=Request.Cookies[name].Value %></li>
    <%}%>
</ul>
