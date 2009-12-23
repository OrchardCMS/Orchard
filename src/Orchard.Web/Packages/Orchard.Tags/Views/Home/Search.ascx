<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TagsSearchViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<% Html.AddTitleParts("Tags", string.Format("Contents tagged with {0}", Model.TagName)); %>
<h2>Contents tagged with <span><%=Html.Encode(Model.TagName) %></span></h2>
<%=Html.UnorderedList(Model.Items, (c, i) => Html.DisplayForItem(c).ToHtmlString(), "contentItems") %>