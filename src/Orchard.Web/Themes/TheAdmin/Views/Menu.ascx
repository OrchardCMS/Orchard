<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<ul id="navigation" role="navigation">
    <%if (Model.AdminMenu != null) {
          foreach (var menuSection in Model.AdminMenu) {
              // todo: (heskew) need some help(er)
              var firstSectionItem = menuSection.Items.FirstOrDefault();
              var sectionHeaderMarkup = firstSectionItem != null
                  ? Html.ActionLink(menuSection.Text, (string)firstSectionItem.RouteValues["action"], firstSectionItem.RouteValues).ToHtmlString()
                  : string.Format("<span>{0}</span>", Html.Encode(menuSection.Text));
              var classification = "";
              if (menuSection == Model.AdminMenu.First())
                  classification = "first ";
              if (menuSection == Model.AdminMenu.Last())
                  classification += "last ";
              
              %>
          <li<%=!string.IsNullOrEmpty(classification) ? string.Format(" class=\"{0}\"", classification.TrimEnd()) : "" %>><h3><%=sectionHeaderMarkup %></h3><ul><%foreach (var menuItem in menuSection.Items) { %>
          <li><%=Html.ActionLink(menuItem.Text, (string)menuItem.RouteValues["action"], menuItem.RouteValues)%></li>
          <%} %></ul></li>
    <%
        }
      }%>
</ul>