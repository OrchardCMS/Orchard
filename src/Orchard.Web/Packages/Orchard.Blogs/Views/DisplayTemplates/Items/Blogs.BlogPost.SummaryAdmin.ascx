<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h2><%=Html.Link(Html.Encode(Model.Item.Title), Url.BlogPostEdit(Model.Item.Blog.Slug, Model.Item.Slug)) %></h2>
<div class="meta"><%=Html.PublishedState(Model.Item) %> | <%=Html.Link(_Encoded("?? comments").ToString(), "") %></div>
<div class="content"><%=Model.Item.As<BodyAspect>().Text ?? string.Format("<p><em>{0}</em></p>", _Encoded("there's no content for this blog post"))%></div>
<p class="actions">
    <%-- todo: (heskew) make into a ul --%>
    <span class="construct">
        <a href="<%=Url.BlogPostEdit(Model.Item.Blog.Slug, Model.Item.Slug) %>" class="ibutton edit"><%=_Encoded("Edit Post")%></a>
        <a href="<%=Url.BlogPost(Model.Item.Blog.Slug, Model.Item.Slug) %>" class="ibutton view"><%=_Encoded("View Post")%></a><%
        if (Model.Item.Published == null) { // todo: (heskew) be smart about this and maybe have other contextual actions - including view/preview for view up there ^^ %>
        <a href="<%=Url.BlogPost(Model.Item.Blog.Slug, Model.Item.Slug) %>" class="ibutton publish"><%=_Encoded("Publish Post Now")%></a>
        <% } %>
    </span>
    <span class="destruct"><a href="#" class="ibutton remove"><%=_Encoded("Remove Post")%></a></span>
</p>