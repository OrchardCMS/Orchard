<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<ContentIndexViewModel>" %>
<%@ Import Namespace="Orchard.DevTools.ViewModels"%>
<h1><%=Html.TitleForPage(T("Content").ToString()) %></h1>
<h2><%=_Encoded("Content Types")%></h2>
<ul>
<%foreach(var item in Model.Types.OrderBy(x=>x.Name)){%>
    <li><%=Html.Encode(item.Name) %></li>
<%}%>
</ul>
<h2><%=_Encoded("Content Items")%></h2>
<ul>
<%foreach(var item in Model.Items.OrderBy(x=>x.Id)){%>
    <li>
        <%=Html.ActionLink(T("{0}: {1}", item.Id, item.ContentType).ToString(), "details", "content", new{item.Id},new{}) %>
        <%=Html.ItemDisplayLink(T("view").ToString(), item) %>
        <%=Html.ItemEditLink(T("edit").ToString(), item) %>
    </li>
<%}%>
</ul>