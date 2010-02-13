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
                sbClass.Append("last ");

            var url = !string.IsNullOrEmpty(menuItem.Url)
                          ? menuItem.Url
                          : Url.RouteUrl(menuItem.RouteValues);

            if (string.Equals(url, Request.Url.AbsolutePath, StringComparison.InvariantCultureIgnoreCase))
                sbClass.Append("current ");
            
            %>
        <li class="<%=sbClass.ToString().TrimEnd() %>"><%=Html.Link(menuItem.Text, url) %></li><%
            ++counter;
        } %>
    </ul>
</div><%
    } %>