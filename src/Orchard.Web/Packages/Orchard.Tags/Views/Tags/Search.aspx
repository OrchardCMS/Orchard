<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsSearchViewModel>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("Header"); %>
    <% Html.BeginForm(); %>
    <div class="yui-g">
						<h2 class="separator">List of contents tagged with <%= Model.TagName %></h2>
						<%=Html.ValidationSummary() %>
			                <% foreach (var contentItem in Model.Contents) { %>
			                <%=Html.ItemDisplayLink(contentItem)%>
			                &nbsp;
                            <% } %>
	</div>
	<% Html.EndForm(); %>
<% Html.Include("Footer"); %>