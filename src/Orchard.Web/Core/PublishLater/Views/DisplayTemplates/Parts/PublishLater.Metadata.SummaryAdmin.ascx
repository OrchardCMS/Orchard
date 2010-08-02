<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.PublishLater.ViewModels.PublishLaterViewModel>" %>
    <ul class="pageStatus">
        <li><%
        // Published or not
        if (Model.HasPublished) { %>
            <img class="icon" src="<%=ResolveUrl("~/Core/PublishLater/Content/Admin/images/online.gif") %>" alt="<%:T("Online") %>" title="<%:T("The page is currently online") %>" /> <%:T("Published") %>&nbsp;&#124;&nbsp;<%
        }
        else { %>
            <img class="icon" src="<%=ResolveUrl("~/Core/PublishLater/Content/Admin/images/offline.gif") %>" alt="<%:T("Offline") %>" title="<%:T("The page is currently offline") %>" /> <%:T("Not Published") %>&nbsp;&#124;&nbsp;<%
        } %>
        </li>
        <li><%
        // Does the page have a draft
        if (Model.HasDraft) { %>
            <img class="icon" src="<%=ResolveUrl("~/Core/PublishLater/Content/Admin/images/draft.gif") %>" alt="<%:T("Draft") %>" title="<%:T("The page has a draft") %>" /><%:T("Draft") %>&nbsp;&#124;&nbsp;<%
        }
        else { %>
            <%:T("No Draft") %>&nbsp;&#124;&nbsp;<%
        } %>
        </li>
        <li><%
    if ((Model.ScheduledPublishUtc.HasValue && Model.ScheduledPublishUtc.Value > DateTime.UtcNow) || (Model.IsPublished && Model.VersionPublishedUtc.HasValue)) {
        if (Model.IsPublished && Model.VersionPublishedUtc.HasValue) { %>
            <%:T("Published: {0}", Html.DateTimeRelative(Model.VersionPublishedUtc.Value, T)) %><%
        }
        else { %>
            <img class="icon" src="<%=ResolveUrl("~/Core/PublishLater/Content/Admin/images/scheduled.gif") %>" alt="<%:T("Scheduled") %>" title="<%:T("The page is scheduled for publishing") %>" /><%:T("Scheduled") %>
        <%:Html.DateTime(Model.ScheduledPublishUtc.Value, T("M/d/yyyy h:mm tt")) %><%
        } %>&nbsp;&#124;&nbsp;</li><%
    } %>
    </ul>