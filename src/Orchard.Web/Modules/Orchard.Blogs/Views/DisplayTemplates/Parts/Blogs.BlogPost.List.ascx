<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Contents.ViewModels.ListContentsViewModel>" %>
<%: Html.UnorderedList(Model.Entries, (bp, i) => Html.DisplayForItem(bp.ViewModel), "blogPosts contentItems") %>
<% if (Model.Entries.Count() < 1) { %><p><%: T("There are no posts for this blog.") %></p><% } %>
