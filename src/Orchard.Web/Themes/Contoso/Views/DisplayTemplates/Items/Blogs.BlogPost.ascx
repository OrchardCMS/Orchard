<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h1 class="page-title"><%=Html.TitleForPage(Model.Item.Title)%></h1>
<div class="metadata">
    <% if (Model.Item.Creator != null) { 
       %><div class="posted"><%=_Encoded("Posted by {0} {1}", Model.Item.Creator.UserName, Html.PublishedWhen(Model.Item)) %></div><%
       } %>
</div>
<% Html.Zone("primary");
   Html.ZonesAny(); %>