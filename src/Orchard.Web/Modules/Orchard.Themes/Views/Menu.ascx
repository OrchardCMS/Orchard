<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Utility.Extensions"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%><%
    var menu = Model.Menu.FirstOrDefault(); 
    if (menu != null && menu.Items.Count() > 0) { %>
    <%-- todo: (heskew) *really* need a better way of doing this. ...and this is really, really ugly :) --%>
    <%-- TODO: (erikpo) Really need a partial for rendering lists (and it should be templatable) to fix the above todo :) --%>
    <ul class="menu"><%
        int counter = 0, count = menu.Items.Count() - 1;
        foreach (var menuItem in menu.Items) {
            var sbClass = new StringBuilder(10);
            if (counter == 0)
                sbClass.Append("first ");
            if (counter == count)
                sbClass.Append("last ");

            if (string.Equals(menuItem.Href, Request.ToUrlString(), StringComparison.InvariantCultureIgnoreCase))
                sbClass.Append("current ");

            var classValue = sbClass.ToString().TrimEnd(); %>
        <li<%=!string.IsNullOrEmpty(classValue) ? string.Format(" class=\"{0}\"", classValue) : "" %>><%=Html.Link(menuItem.Text, menuItem.Href) %></li><%
            ++counter;
        } %>
    </ul>
<%
    } %>