<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%><%
if (Model.Creator != null) { 
    %><%: T("Posted by {0} {1}", Model.Creator.UserName, Html.PublishedWhen(Model, T)) %><%
} %>