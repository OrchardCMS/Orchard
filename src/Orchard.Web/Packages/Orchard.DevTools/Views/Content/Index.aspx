<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.DevTools.ViewModels.ContentIndexViewModel>" %>


<h3>Content Types</h3>
<ul>
<%foreach(var item in Model.Types.OrderBy(x=>x.Name)){%>
<li><%=Html.Encode(item.Name) %> </li>
<%}%>
</ul>

<h3>Content Items</h3>
<ul>
<%foreach(var item in Model.Items.OrderBy(x=>x.Id)){%>
<li><%=Html.ActionLink(item.Id+": "+item.ContentType, "details", "content", new{item.Id},new{}) %>
<%=Html.ItemDisplayLink("view", item) %>
 <%=Html.ItemEditLink("edit", item) %></li>
<%}%>
</ul>

