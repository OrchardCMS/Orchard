<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemDisplayModel<Blog>>" %>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<h3><%=Html.Link(Html.Encode(Model.Item.Name), Url.Blog(Model.Item.Slug)) %></h3>
<div class="blog metadata"><a href="<%=Url.Blog(Model.Item.Slug) %>"><%=Model.Item.PostCount %> post<%=Model.Item.PostCount == 1 ? "" : "s" %></a></div>
<p><%=Model.Item.Description %></p>
<%--TODO: (erikpo) Need to figure out which zones should be displayed in this template--%>
<%--<%=Html.DisplayZonesAny() %>--%>