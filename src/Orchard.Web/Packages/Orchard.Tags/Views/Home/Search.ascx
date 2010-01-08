<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<TagsSearchViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<% Html.AddTitleParts(T("Tags").ToString(), T("Contents tagged with {0}", Model.TagName).ToString()); %>
<h1><%=T("Contents tagged with <span>{0}</span>", Html.Encode(Model.TagName)) %></h1>
<%=Html.UnorderedList(Model.Items, (c, i) => Html.DisplayForItem(c).ToHtmlString(), "contentItems") %>