<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemDisplayViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Models.ViewModels"%>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>

<h2><%=Html.Encode(Model.Item.Name) %></h2>
<div><%=Html.Encode(Model.Item.Description) %></div>
<%=Html.DisplayZonesAny() %>

<%--    <div class="manage"><a href="<%=Url.BlogEdit(Model.Blog.Slug) %>" class="ibutton edit">edit</a></div>
    <h2><%=Html.Encode(Model.Blog.Name) %></h2>
    <div><%=Html.Encode(Model.Blog.Description) %></div>
    <%=Html.UnorderedList(Model.Posts, (bp, i) => Html.DisplayForItem(bp).ToHtmlString(), "posts contentItems") %>
--%>