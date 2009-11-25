<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("Header"); %>
    <div class="yui-g">
						<h2 class="separator">Tags</h2>
						<%=Html.ValidationSummary() %>
                        <% foreach (var tag in Model.Tags) { %>
                            <%=Html.ActionLink(tag.TagName, "Search", new {tagName = tag.TagName}, new {@class="floatRight topSpacer"}) %>
                            &nbsp;
                        <% } %>
	</div>
<% Html.Include("Footer"); %>