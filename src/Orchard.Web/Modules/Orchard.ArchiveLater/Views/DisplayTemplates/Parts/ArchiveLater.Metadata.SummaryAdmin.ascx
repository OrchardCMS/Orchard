<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ArchiveLater.ViewModels.ArchiveLaterViewModel>" %>
    <%
    if ((Model.IsPublished && Model.ScheduledArchiveUtc.HasValue && Model.ScheduledArchiveUtc.Value > DateTime.UtcNow)) {%>
    <ul class="pageStatus">
        <li>
            <img class="icon" src="<%=ResolveUrl("~/Modules/ArchiveLater/Content/Admin/images/scheduled.gif") %>" alt="<%:T("Scheduled") %>" title="<%:T("The page is scheduled for archiving") %>" /><%:T("Unpublish on") %>
            <%:Html.DateTime(Model.ScheduledArchiveUtc.Value.ToLocalTime(), T("M/d/yyyy h:mm tt"))%>
            &nbsp;&#124;&nbsp;
        </li>
    </ul><% 
    } %>
