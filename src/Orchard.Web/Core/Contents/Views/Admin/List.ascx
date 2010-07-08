<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Contents.ViewModels.ListContentsViewModel>" %>
<%@ Import Namespace="Orchard.ContentManagement.Aspects"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Utility.Extensions" %>
<h1><%:Html.TitleForPage(T("Manage {0} Content", !string.IsNullOrEmpty(Model.TypeDisplayName) ? Model.TypeDisplayName : T("all").Text).ToString())%></h1>
<div class="manage">
    <%:Html.ActionLink(!string.IsNullOrEmpty(Model.TypeDisplayName) ? T("Add new {0} content", Model.TypeDisplayName).Text : T("Add new content").Text, "Create", new { }, new { @class = "button primaryAction" })%>
</div>
<ul class="contentItems"><%
    foreach (var entry in Model.Entries) { %>
        <li>
            <div class="summary">
                <div class="properties">
                    <h3><%:entry.ContentItem.Is<IRoutableAspect>()
                            ? Html.ActionLink(entry.ContentItem.As<IRoutableAspect>().Title, "Edit", new { id = entry.ContentItem.Id })
                            : MvcHtmlString.Create(string.Format("[title display template needed] (content type == \"{0}\")", entry.ContentItem.TypeDefinition.Name)) %></h3>
                    <ul class="pageStatus">
                        <li>
                            <%:T("Last modified: {0}",
                                entry.ContentItem.Is<ICommonAspect>() && entry.ContentItem.As<ICommonAspect>().ModifiedUtc.HasValue
                                    ? Html.DateTimeRelative(entry.ContentItem.As<ICommonAspect>().ModifiedUtc.Value, T)
                                    : T("unknown"))%>
                        </li>
                    </ul>
                </div>
                <div class="related">
                    <%:Html.ActionLink(T("Edit").ToString(), "Edit", new { id = entry.ContentItem.Id }, new { title = T("Edit").ToString() })%><%: T(" | ")%>
                    <% using (Html.BeginFormAntiForgeryPost(Url.Action("Remove", new { id = entry.ContentItem.Id }), FormMethod.Post, new { @class = "inline link" })) { %>
                    <button type="submit" class="linkButton" title="<%: T("Remove") %>"><%: T("Remove") %></button>
                    <%:Html.Hidden("returnUrl", ViewContext.RequestContext.HttpContext.Request.ToUrlString())%><%
                    } %>
                </div>
                <div style="clear:both;"></div>
            </div>
        </li><%
    } %>
</ul>