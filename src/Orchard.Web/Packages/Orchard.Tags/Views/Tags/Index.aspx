<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("Header"); %>
    <div class="yui-g">
						<h2 class="separator">Tags</h2>
						<%=Html.ValidationSummary() %>
                        <% foreach (var tag in Model.Tags) { %>
                            <%=Html.ActionLink(tag.TagName, "TagName", new {tagId = tag.Id}, new {@class="floatRight topSpacer"}) %>
                            &nbsp;
                        <% } %>
	</div>
<% Html.Include("Footer"); %>