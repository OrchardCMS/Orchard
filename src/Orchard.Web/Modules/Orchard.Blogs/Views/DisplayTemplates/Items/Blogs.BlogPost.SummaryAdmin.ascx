<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.ContentManagement.Aspects"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<div class="summary">
    <div class="properties">
        <h3><%=Html.Link(Html.Encode(Model.Item.Title), Url.BlogPostEdit(Model.Item))%></h3>
        <ul>
            <li><%
            if (Model.Item.HasPublished) { %>
                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Blogs/Content/Admin/images/online.gif") %>" alt="<%: T("Online") %>" title="<%: T("The page is currently online") %>" /><%: T(" Published")%><%
            }
            else { %>
                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Blogs/Content/Admin/images/offline.gif") %>" alt="<%: T("Offline") %>" title="<%: T("The page is currently offline") %>" /><%: T(" Not Published")%><%
            } %>&nbsp;&#124;&nbsp;
            </li>
            <li><%
            if (Model.Item.HasDraft) { %>
                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Blogs/Content/Admin/images/draft.gif") %>" alt="<%: T("Draft") %>" title="<%: T("The post has a draft") %>" /><%=Html.PublishedState(Model.Item)%><%
            }
            else { %>
                <%: T("No draft")%><%
            } %>&nbsp;&#124;&nbsp;
            </li>
            <li><%
            if (Model.Item.ScheduledPublishUtc.HasValue && Model.Item.ScheduledPublishUtc.Value > DateTime.UtcNow) { %>
                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Blogs/Content/Admin/images/scheduled.gif") %>" alt="<%: T("Scheduled") %>" title="<%: T("The post is scheduled for publishing") %>" /><%: T("Scheduled")%>
                <%=Html.DateTime(Model.Item.ScheduledPublishUtc.Value, "M/d/yyyy h:mm tt")%><%
            }
            else if (Model.Item.IsPublished) { %>
                <%: T("Published: ") + Html.DateTimeRelative(Model.Item.As<ICommonAspect>().VersionPublishedUtc.Value)%><%
            }
            else { %>
                <%: T("Last modified: ") + Html.DateTimeRelative(Model.Item.As<ICommonAspect>().ModifiedUtc.Value) %><%
            } %>&nbsp;&#124;&nbsp;
            </li>
            <li><%: T("By {0}", Model.Item.Creator.UserName)%></li>                   
        </ul>
    </div>
    <div class="related"><%
        if (Model.Item.HasPublished){ %>
        <a href="<%=Url.BlogPost(Model.Item) %>" title="<%: T("View Post")%>"><%: T("View")%></a><%: T(" | ")%><%
            if (Model.Item.HasDraft) { %>
        <a href="<%=Html.AntiForgeryTokenGetUrl(Url.BlogPostPublish(Model.Item)) %>" title="<%: T("Publish Draft")%>"><%: T("Publish Draft")%></a><%: T(" | ")%><%
            } %>
        <a href="<%=Html.AntiForgeryTokenGetUrl(Url.BlogPostUnpublish(Model.Item)) %>" title="<%: T("Unpublish Post")%>"><%: T("Unpublish")%></a><%: T(" | ")%><%
        }
        else { %>
        <a href="<%=Html.AntiForgeryTokenGetUrl(Url.BlogPostPublish(Model.Item)) %>" title="<%: T("Publish Post")%>"><%: T("Publish")%></a><%: T(" | ")%><%
        } %>
        <a href="<%=Url.BlogPostEdit(Model.Item) %>" title="<%: T("Edit Post")%>"><%: T("Edit")%></a><%: T(" | ")%>
        <a href="<%=Html.AntiForgeryTokenGetUrl(Url.BlogPostDelete(Model.Item)) %>" title="<%: T("Remove Post")%>"><%: T("Remove")%></a>
        <br /><%Html.Zone("meta");%>
    </div>
    <div style="clear:both;"></div>
</div>