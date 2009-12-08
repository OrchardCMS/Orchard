<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ItemDisplayViewModel<BlogPost>>>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%@ Import Namespace="Orchard.Models.ViewModels" %>
<%@ Import Namespace="Orchard.Models" %>
<%@ Import Namespace="Orchard.Blogs.Extensions" %>
<%@ Import Namespace="Orchard.Core.Common.Models" %>
<%@ Import Namespace="Orchard.Blogs.Models" %>
<ul class="posts contentItems">
<%foreach (var item in Model) { %>
<li>
<%= Html.DisplayForItem(item)%>
</li>
<%} %>
</ul>
