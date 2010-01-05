<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ItemViewModel<BlogPost>>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%=Html.UnorderedList(Model, (bp, i) => Html.DisplayForItem(bp).ToHtmlString(), "blogPosts contentItems") %>