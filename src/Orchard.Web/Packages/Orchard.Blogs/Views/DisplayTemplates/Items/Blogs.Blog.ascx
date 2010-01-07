<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Blog>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%-- todo: (heskew) selectively display to those who have access --%>
<div class="manage"><a href="<%=Url.BlogEdit(Model.Item.Slug) %>" class="ibutton edit"><%=_Encoded("edit") %></a></div>
<h1><%=Html.TitleForPage(Model.Item.Name) %></h1>
<p><%=Html.Encode(Model.Item.Description) %></p>
<% Html.Zone("primary");
   Html.ZonesAny(); %>