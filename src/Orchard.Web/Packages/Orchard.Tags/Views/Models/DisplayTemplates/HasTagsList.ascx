<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%@ Import Namespace="Orchard.Tags.Models" %>
<p class="tags">
<% if (Model.CurrentTags.Count > 0) { %><span>Tags:</span> <% } %>
<%=string.Join(", ", Model.CurrentTags.Select(t => Html.ActionLink(t.TagName, "Search", "Home", new { area = "Orchard.Tags", tagName = t.TagName }, new { }).ToHtmlString()).ToArray())%>
<%--<%=Html.UnorderedList(Model.CurrentTags, (t, i) => Html.ActionLink(t.TagName, "Search", "Home", new { area = "Orchard.Tags", tagName = t.TagName }, new { }).ToHtmlString(), "tags")%>--%>
</p>