<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<div class="metadata"><%
    if (Model.Creator != null) { 
       %><div class="posted"><%: T("Posted by {0} {1}", Model.Creator.UserName, Html.PublishedWhen(Model)) %></div><%
    } %>
</div>