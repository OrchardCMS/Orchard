<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><a href="<%=Url.BlogForAdmin(Model.Item.Slug) %>"><%=Html.Encode(Model.Item.Name) %></a></h3>
<div class="meta">
    <% var postCount = Model.Item.PostCount; %><a href="<%=Url.BlogForAdmin(Model.Item.Slug) %>"><%=string.Format("{0} post{1}", postCount, postCount == 1 ? "" : "s") %></a>
    | <a href="#">?? comments</a>
</div>
<%--<p>[list of authors] [modify blog access]</p>--%>
<p><%=Model.Item.Description %></p>
<p class="actions">
    <%-- todo: (heskew) make into a ul --%>
    <span class="construct">
        <a href="<%=Url.BlogForAdmin(Model.Item.Slug) %>" class="ibutton blog" title="Manage Blog">Manage Blog</a>
        <a href="<%=Url.BlogEdit(Model.Item.Slug) %>" class="ibutton edit" title="Edit Blog">Edit Blog</a>
        <a href="<%=Url.Blog(Model.Item.Slug) %>" class="ibutton view" title="View Blog">View Blog</a>
        <a href="<%=Url.BlogPostCreate(Model.Item.Slug) %>" class="ibutton add page" title="New Post">New Post</a>
    </span>
    <span class="destruct"><a href="<%=Url.BlogDelete(Model.Item.Slug) %>" class="ibutton remove" title="Delete Blog">Remove Blog</a></span>
</p>