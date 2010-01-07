<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%-- todo: (heskew) get what actions we can out of the h2 :| --%>
<h1 class="withActions">
    <a href="<%=Url.BlogForAdmin(Model.Item.Slug) %>"><%=Html.TitleForPage(Model.Item.Name) %></a>
    <a href="<%=Url.BlogEdit(Model.Item.Slug) %>" class="ibutton edit"><%=_Encoded("Edit Blog") %></a>
    <span class="actions"><span class="destruct"><a href="<%=Url.BlogDelete(Model.Item.Slug) %>" class="ibutton remove"><%=_Encoded("Remove Blog") %></a></span></span></h1>
<p><%=Html.Encode(Model.Item.Description) %></p>
<div class="actions"><a href="<%=Url.BlogPostCreate(Model.Item.Slug) %>" class="add button"><%=_Encoded("New Post")%></a></div>
<% Html.Zone("primary");
   Html.ZonesAny(); %>