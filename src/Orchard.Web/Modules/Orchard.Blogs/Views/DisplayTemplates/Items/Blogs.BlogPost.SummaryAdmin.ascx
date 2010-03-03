<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
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
                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Blogs/Content/Admin/images/online.gif") %>" alt="<%=_Encoded("Online") %>" title="<%=_Encoded("The page is currently online") %>" /><%=_Encoded(" Published")%><%
            }
            else { %>
                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Blogs/Content/Admin/images/offline.gif") %>" alt="<%=_Encoded("Offline") %>" title="<%=_Encoded("The page is currently offline") %>" /><%=_Encoded(" Not Published")%><%
            } %>&nbsp;&#124;&nbsp;
            </li>
            <li><%
            if (Model.Item.HasDraft) { %>
                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Blogs/Content/Admin/images/draft.gif") %>" alt="<%=_Encoded("Draft") %>" title="<%=_Encoded("The post has a draft") %>" /><%=Html.PublishedState(Model.Item)%><%
            }
            else { %>
                <%=_Encoded("No draft")%><%
            } %>&nbsp;&#124;&nbsp;
            </li>
            <li><%
            if (Model.Item.ScheduledPublishUtc.HasValue && Model.Item.ScheduledPublishUtc.Value > DateTime.UtcNow) { %>
                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Blogs/Content/Admin/images/scheduled.gif") %>" alt="<%=_Encoded("Scheduled") %>" title="<%=_Encoded("The post is scheduled for publishing") %>" /><%=_Encoded("Scheduled")%>
                <%=Html.DateTime(Model.Item.ScheduledPublishUtc.Value, "M/d/yyyy h:mm tt")%><%
            }
            else if (Model.Item.IsPublished) { %>
                <%=_Encoded("Published: ") + Html.PublishedWhen(Model.Item) %><%
            }
            else { %>
                <%=_Encoded("Last modified: ") + Html.DateTimeRelative(Model.Item.As<CommonAspect>().ModifiedUtc.Value) %><%
            } %>&nbsp;&#124;&nbsp;
            </li>
            <li><%=_Encoded("By {0}", Model.Item.Creator == null ? String.Empty : Model.Item.Creator.UserName)%></li>                   
        </ul>
    </div>
    <div class="related"><%
        if (Model.Item.HasPublished){ %>
        <a href="<%=Url.BlogPost(Model.Item) %>" title="<%=_Encoded("View Post")%>"><%=_Encoded("View")%></a><%=_Encoded(" | ")%><%
            if (Model.Item.HasDraft) { %>
            <a href="<%=Html.AntiForgeryTokenGetUrl(Url.BlogPostPublish(Model.Item)) %>" title="<%=_Encoded("Publish Draft")%>"><%=_Encoded("Publish Draft")%></a><%=_Encoded(" | ")%><%
            } %>
        <a href="<%=Html.AntiForgeryTokenGetUrl(Url.BlogPostUnpublish(Model.Item)) %>" title="<%=_Encoded("Unpublish Post")%>"><%=_Encoded("Unpublish")%></a><%=_Encoded(" | ")%><%
        }
        else { %>
        <a href="<%=Html.AntiForgeryTokenGetUrl(Url.BlogPostPublish(Model.Item)) %>" title="<%=_Encoded("Publish Post")%>"><%=_Encoded("Publish")%></a><%=_Encoded(" | ")%><%
        } %>
        <a href="<%=Url.BlogPostEdit(Model.Item) %>" title="<%=_Encoded("Edit Post")%>"><%=_Encoded("Edit")%></a><%=_Encoded(" | ")%>
        <a href="<%=Html.AntiForgeryTokenGetUrl(Url.BlogPostDelete(Model.Item)) %>" title="<%=_Encoded("Delete")%>"><%=_Encoded("Delete")%></a>
        <br /><%Html.Zone("meta");%>
    </div>
    <div style="clear:both;"></div>
</div>