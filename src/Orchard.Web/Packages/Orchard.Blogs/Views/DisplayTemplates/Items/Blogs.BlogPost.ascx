<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemDisplayModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<div class="manage"><a href="<%=Url.BlogPostEdit(Model.Item.Blog.Slug, Model.Item.Slug) %>" class="ibutton edit">edit</a></div>
<h1><%=Html.TitleForPage(Model.Item.Title)%></h1>
<div class="metadata">
    <% if (Model.Item.Creator != null)
       { 
       %><div class="posted">Posted by <%=Html.Encode(Model.Item.Creator.UserName)%> <%=Html.PublishedWhen(Model.Item)%></div><%
       } %>
</div>
<% Html.ZonesAny(); %>