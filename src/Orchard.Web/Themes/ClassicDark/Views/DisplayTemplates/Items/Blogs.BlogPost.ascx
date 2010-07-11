<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h1><%: Html.TitleForPage(Model.Item.Title)%></h1>
<%-- Sorry, Jon. I need to figure out how we can make this markup possible with the recent metadata/manage split.
    We can still do it this way but there's isn't yet a story for UI conditional on permissions.
    What I have in this template is as close as I can get at the moment. --%>
<%--<div class="metadata">
    <% if (Model.Item.Creator != null) { 
       %><div class="posted"><%: T("Posted by {0} {1}", Model.Item.Creator.UserName, Html.PublishedWhen(Model.Item)) %><% --  | <a href="<%: Url.BlogPostEdit(Model.Item.Blog.Slug, Model.Item.Id) %>" class="ibutton edit"><%: T("Edit") %></a>-- %></div><%
       } %>
</div>
--%>
<% Html.Zone("primary"); %>