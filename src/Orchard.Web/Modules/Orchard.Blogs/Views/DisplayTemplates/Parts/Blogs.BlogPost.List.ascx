<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<IEnumerable<ContentItemViewModel<BlogPost>>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%=Html.UnorderedList(Model, (bp, i) => Html.DisplayForItem(bp).ToHtmlString(), "blogPosts contentItems") %>
<% if (Model.Count() < 1) { %><div class="info message"><%: T("There are no posts for this blog.") %></div><% } %>
