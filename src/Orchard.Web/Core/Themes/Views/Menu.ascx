<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%><%
    var menu = Model.Menu.FirstOrDefault(); 
    if (menu != null && menu.Items.Count() > 0) { %>
<div id="menucontainer">
    <%-- todo: (heskew) *really* need a better way of doing this. ...and this is really, really ugly :) --%>
    <%-- TODO: (erikpo) Really need a partial for rendering lists (and it should be templatable) to fix the above todo :) --%>
    <ul id="menu"><%
        int counter = 0, count = menu.Items.Count() - 1;
        foreach (var menuItem in menu.Items) {
            var sbClass = new StringBuilder(10);
            if (counter == 0)
                sbClass.Append("first ");
            if (counter == count)
                sbClass.Append("last ");%>
        <li class="<%=sbClass.ToString().TrimEnd() %>"><%=!string.IsNullOrEmpty(menuItem.Url)
                ? Html.Link(menuItem.Text, ResolveUrl(menuItem.Url))
                : Html.ActionLink(menuItem.Text, (string)menuItem.RouteValues["action"], menuItem.RouteValues).ToHtmlString() %></li><%
            ++counter;
        } %>
    </ul>
</div><%
    } %>