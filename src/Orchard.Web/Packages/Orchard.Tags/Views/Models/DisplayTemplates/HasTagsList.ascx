<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%@ Import Namespace="Orchard.Tags.Models" %>
Tag<%=Model.CurrentTags.Count == 1 ? "" : "s" %>: <%
    int tagCount = 0;
    foreach (Tag tag in Model.CurrentTags) {
        if (tagCount > 0) {
            %>, <%
        }
        %><%=Html.ActionLink(tag.TagName, "Search", "Home", new{ area="Orchard.Tags", tagName=tag.TagName}, new {}) %><%
        tagCount++;
    } %>