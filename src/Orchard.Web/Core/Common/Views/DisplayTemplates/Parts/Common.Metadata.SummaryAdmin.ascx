<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Common.ViewModels.CommonMetadataViewModel>" %>
    <ul class="pageStatus">
        <li><%
        // Published or not
        if (Model.HasPublished) { %>
            <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/online.gif") %>" alt="<%:T("Online") %>" title="<%:T("The page is currently online") %>" /> <%:T("Published") %>&nbsp;&#124;&nbsp;<%
        }
        else { %>
            <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/offline.gif") %>" alt="<%:T("Offline") %>" title="<%:T("The page is currently offline") %>" /> <%:T("Not Published") %>&nbsp;&#124;&nbsp;<%
        } %>
        </li>
        <li><%
        // Does the page have a draft
        if (Model.HasDraft) { %>
            <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/draft.gif") %>" alt="<%:T("Draft") %>" title="<%:T("The page has a draft") %>" /><%:T("Draft") %>&nbsp;&#124;&nbsp;<%
        }
        else { %>
            <%:T("No Draft") %>&nbsp;&#124;&nbsp;<%
        } %>
        </li>
        <li><%
        if (Model.ScheduledPublishUtc.HasValue && Model.ScheduledPublishUtc.Value > DateTime.UtcNow) { %>
            <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/scheduled.gif") %>" alt="<%:T("Scheduled") %>" title="<%:T("The page is scheduled for publishing") %>" /><%:T("Scheduled") %>
            <%:Html.DateTime(Model.ScheduledPublishUtc.Value, T("M/d/yyyy h:mm tt")) %><%
        }
        else if (Model.IsPublished && Model.VersionPublishedUtc.HasValue) { %>
            <%:T("Published: {0}", Html.DateTimeRelative(Model.VersionPublishedUtc.Value, T)) %><%
        }
        else if (Model.ModifiedUtc.HasValue) { %>
            <%:T("Last modified: {0}", Html.DateTimeRelative(Model.ModifiedUtc.Value, T)) %><%
        } %>&nbsp;&#124;&nbsp;
        </li>
        <li><%:T("By {0}", Model.Creator.UserName) %></li>
    </ul>