<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPart>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Core.Common.Extensions" %><%
if (Model.Creator != null) { 
    %><%: T("Posted by {0} {1}", Model.Creator.UserName, Display.PublishedWhen(versionPublishedUtc: Model))%><%
} %>