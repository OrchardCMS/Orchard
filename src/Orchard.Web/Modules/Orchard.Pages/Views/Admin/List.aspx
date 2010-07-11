<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<PagesViewModel>" %>
<%@ Import Namespace="Orchard.ContentManagement.Aspects"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Pages.ViewModels"%><%
Html.RegisterStyle("admin.css"); %>

<h1><%: Html.TitleForPage(T("Manage Pages").ToString())%></h1>
<%-- todo: Add helper text here when ready. <p><%: T("Possible text about setting up a page goes here.")%></p>--%>
<div class="manage"><%: Html.ActionLink(T("Add a page").ToString(), "Create", new { }, new { @class = "button primaryAction" })%></div><%
using (Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary()%>
    <fieldset class="actions bulk">
        <label for="publishActions"><%: T("Actions:")%></label>
        <select id="publishActions" name="<%: Html.NameOf(m => m.Options.BulkAction) %>">
            <%: Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.None, T("Choose action...").ToString())%>
            <%: Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.PublishNow, T("Publish Now").ToString())%>
            <%: Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.Unpublish, T("Unpublish").ToString())%>
            <%: Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.Delete, T("Remove").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.BulkEdit" value="<%: T("Apply") %>" />
    </fieldset>
    <fieldset class="actions bulk">
        <label for="filterResults"><%: T("Filter:")%></label>
        <select id="filterResults" name="<%: Html.NameOf(m => m.Options.Filter) %>">
            <%: Html.SelectOption(Model.Options.Filter, PagesFilter.All, T("All Pages").ToString())%>
            <%: Html.SelectOption(Model.Options.Filter, PagesFilter.Published, T("Published Pages").ToString())%>
            <%: Html.SelectOption(Model.Options.Filter, PagesFilter.Offline, T("Offline Pages").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.Filter" value="<%: T("Apply") %>"/>
    </fieldset>
    <fieldset class="pageList">
        <ul class="contentItems"><%
        var pageIndex = 0;
        foreach (var pageEntry in Model.PageEntries) {
            var pi = pageIndex; %>
            <li>
                <div class="summary">
                    <div class="properties">
                        <input type="hidden" value="<%=Model.PageEntries[pageIndex].PageId %>" name="<%: Html.NameOf(m => m.PageEntries[pi].PageId) %>"/>
                        <input type="checkbox" value="true" name="<%: Html.NameOf(m => m.PageEntries[pi].IsChecked) %>"/>
                        <h3><%: Html.ActionLink(pageEntry.Page.Title, "Edit", new { id = pageEntry.PageId })%></h3>
                        <ul class="pageStatus">
                            <li><%
                            // Published or not
                            if (pageEntry.Page.HasPublished) { %>
                                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/online.gif") %>" alt="<%: T("Online") %>" title="<%: T("The page is currently online") %>" /> <%: T("Published") %>&nbsp;&#124;&nbsp;<%
                            }
                            else { %>
                                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/offline.gif") %>" alt="<%: T("Offline") %>" title="<%: T("The page is currently offline") %>" /> <%: T("Not Published")%>&nbsp;&#124;&nbsp;<%
                            } %>
                            </li>
                            <li><%
                            // Does the page have a draft
                            if (pageEntry.Page.HasDraft) { %>
                                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/draft.gif") %>" alt="<%: T("Draft") %>" title="<%: T("The page has a draft") %>" /><%: T("Draft")%>&nbsp;&#124;&nbsp;<%
                            }
                            else { %>
                                <%: T("No Draft")%>&nbsp;&#124;&nbsp;<%
                            } %>
                            </li>
                            <li><%
                            if (pageEntry.Page.ScheduledPublishUtc.HasValue && pageEntry.Page.ScheduledPublishUtc.Value > DateTime.UtcNow) { %>
                                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/scheduled.gif") %>" alt="<%: T("Scheduled") %>" title="<%: T("The page is scheduled for publishing") %>" /><%: T("Scheduled")%>
                                <%: Html.DateTime(pageEntry.Page.ScheduledPublishUtc.Value, T("M/d/yyyy h:mm tt"))%><%
                            }
                            else if (pageEntry.Page.IsPublished) { %>
                                <%: T("Published: {0}", Html.DateTimeRelative(pageEntry.Page.As<ICommonAspect>().VersionPublishedUtc.Value, T)) %><%
                            }
                            else { %>
                                <%: T("Last modified: {0}", Html.DateTimeRelative(pageEntry.Page.As<ICommonAspect>().ModifiedUtc.Value, T)) %><%
                            } %>&nbsp;&#124;&nbsp;
                            </li>
                            <li><%: T("By {0}", pageEntry.Page.Creator.UserName)%></li>
                        </ul>
                    </div>
                    <div class="related"><%
                        if (pageEntry.Page.HasPublished) { %>
                        <%: Html.ActionLink("View", "Item", new { controller = "Page", slug = pageEntry.Page.PublishedSlug }, new {title = T("View Page")})%><%: T(" | ")%><%
                            if (pageEntry.Page.HasDraft) { %>
                        <a href="<%: Html.AntiForgeryTokenGetUrl(Url.Action("Publish", new {id = pageEntry.Page.Id})) %>" title="<%: T("Publish Draft")%>"><%: T("Publish Draft")%></a><%: T(" | ")%><%
                            } %>
                        <a href="<%: Html.AntiForgeryTokenGetUrl(Url.Action("Unpublish", new {id = pageEntry.Page.Id})) %>" title="<%: T("Unpublish Page")%>"><%: T("Unpublish")%></a><%: T(" | ")%><%
                        }
                        else { %>
                        <a href="<%: Html.AntiForgeryTokenGetUrl(Url.Action("Publish", new {id = pageEntry.Page.Id})) %>" title="<%: T("Publish Page")%>"><%: T("Publish")%></a><%: T(" | ")%><%
                        } %>
                        <%: Html.ActionLink(T("Edit").ToString(), "Edit", new { id = pageEntry.Page.Id }, new { title = T("Edit Page").ToString() })%><%: T(" | ")%>
                        <a href="<%: Html.AntiForgeryTokenGetUrl(Url.Action("Delete", new {id = pageEntry.Page.Id})) %>" title="<%: T("Remove Page")%>"><%: T("Remove")%></a>
                    </div>
                    <div style="clear:both;"></div>
                </div>
            </li><%
            pageIndex++;
        } %>
        </ul>
    </fieldset><%
} %>
<div class="manage"><%: Html.ActionLink(T("Add a page").ToString(), "Create", new { }, new { @class = "button primaryAction" })%></div>
