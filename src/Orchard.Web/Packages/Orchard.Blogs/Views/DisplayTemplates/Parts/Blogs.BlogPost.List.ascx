<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ItemDisplayModel<BlogPost>>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%=Html.UnorderedList(Model, (bp, i) => Html.DisplayForItem(bp).ToHtmlString(), "blogPosts contentItems") %>