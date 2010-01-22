<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Comments.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%
    Html.AddTitleParts(Model.Blog.Name);
    var blogPost = Model.BlogPost.Item;
    var bodyViewModelModel = new BodyDisplayViewModel { BodyAspect = blogPost.ContentItem.As<BodyAspect>() };
    var hasComments = blogPost.ContentItem.As<HasComments>(); %>
<div class="manage"><a href="<%=Url.BlogPostEdit(blogPost.Blog.Slug, blogPost.Id) %>" class="ibutton edit"><%=_Encoded("edit") %></a></div>
<h1><%=Html.TitleForPage(blogPost.Title)%></h1>
<div class="metadata">
    <% if (blogPost.Creator != null) { 
       %><div class="posted"><%=_Encoded("Posted by {0} {1}", blogPost.Creator.UserName, Html.PublishedWhen(blogPost))%></div><%
       } %>
    <%=Html.Link(T(hasComments.Comments.Count == 1 ? "{0} comment" : "{0} comments", hasComments.Comments.Count).ToString(), "#comments")%>
</div>
<%=Html.DisplayFor(m => bodyViewModelModel, "Parts/Common.Body")%>
<%=Html.DisplayFor(m => hasComments, "Parts/Comments.HasComments", "") %>