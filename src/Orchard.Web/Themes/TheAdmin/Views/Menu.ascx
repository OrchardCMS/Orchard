<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<ul id="navigation" role="navigation">
    <li class="first"><h3><span><%=_Encoded("Dashboard")%></span></h3></li>
    <%if (Model.Menu != null) {
          foreach (var menuSection in Model.Menu) {
              // todo: (heskew) need some help(er)
              var firstSectionItem = menuSection.Items.FirstOrDefault();
              var sectionHeaderMarkup = firstSectionItem != null
                  ? Html.ActionLink(menuSection.Text, (string)firstSectionItem.RouteValues["action"], firstSectionItem.RouteValues).ToHtmlString()
                  : string.Format("<span>{0}</span>", Html.Encode(menuSection.Text));
              %>
          <li><h3><%=sectionHeaderMarkup %></h3><ul><%foreach (var menuItem in menuSection.Items) { %>
          <li><%=Html.ActionLink(menuItem.Text, (string)menuItem.RouteValues["action"], menuItem.RouteValues)%></li>
          <%} %></ul></li>
    <%
        }
      }%>
</ul>