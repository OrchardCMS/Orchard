<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<PagesViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Pages.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Pages").ToString())%></h1>
<%-- todo: Add helper text here when ready. <p><%=_Encoded("Possible text about setting up a page goes here.")%></p>--%>
<div class="manage"><%=Html.ActionLink(T("Add a page").ToString(), "Create", new { }, new { @class = "button" })%></div>
<% using (Html.BeginFormAntiForgeryPost())
   { %>
    <%=Html.ValidationSummary()%>
    <fieldset class="actions bulk">
        <label for="publishActions"><%=_Encoded("Actions:")%></label>
        <select id="publishActions" name="<%=Html.NameOf(m => m.Options.BulkAction) %>">
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.None, _Encoded("Choose action...").ToString())%>
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.PublishNow, _Encoded("Publish Now").ToString())%>
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.Unpublish, _Encoded("Unpublish").ToString())%>
            <%=Html.SelectOption(Model.Options.BulkAction, PagesBulkAction.Delete, _Encoded("Delete").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.BulkEdit" value="<%=_Encoded("Apply") %>" />
    </fieldset>
    <fieldset class="actions bulk">
        <label for="filterResults"><%=_Encoded("Filter:")%></label>
        <select id="filterResults" name="<%=Html.NameOf(m => m.Options.Filter) %>">
            <%=Html.SelectOption(Model.Options.Filter, PagesFilter.All, _Encoded("All Pages").ToString())%>
            <%=Html.SelectOption(Model.Options.Filter, PagesFilter.Published, _Encoded("Published Pages").ToString())%>
            <%=Html.SelectOption(Model.Options.Filter, PagesFilter.Offline, _Encoded("Offline Pages").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.Filter" value="<%=_Encoded("Apply") %>"/>
    </fieldset>
    <fieldset>       
         <ul class="contentItems" style="margin-top:0;"> 
            <%
int pageIndex = 0;
foreach (var pageEntry in Model.PageEntries)
{
    var pi = pageIndex; %>
<li style="padding:1em .5em;">
<div style="float:left;">
                    <input type="hidden" value="<%=Model.PageEntries[pageIndex].PageId %>" name="<%=Html.NameOf(m => m.PageEntries[pi].PageId) %>"/>
                    <input type="checkbox" value="true" name="<%=Html.NameOf(m => m.PageEntries[pi].IsChecked) %>"/>
                    
                    <h3 style="border-bottom:none; margin:0; padding:0; display:inline;"><%=Html.Encode(pageEntry.Page.Title ?? T("(no title)").ToString())%></h3>
                    
<p style="margin:.5em 0;">

<%--Published or not--%>
                  <% if (pageEntry.Page.HasPublished)
                     { %>
                  <img src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/online.gif") %>" alt="<%=_Encoded("Online") %>" title="<%=_Encoded("The page is currently online") %>" style="<%=_Encoded("margin:0 0 -2px 0;") %>" /><%=_Encoded(" Published |") %>
                  <% }
                     else
                     { %>
                  <img src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/offline.gif") %>" alt="<%=_Encoded("Offline") %>" title="<%=_Encoded("The page is currently offline") %>" style="margin:0 0 -2px 0;" /><%=_Encoded(" Not Published |")%>
                  <% } %>

<%--Does the page have a draft--%>
<% if (pageEntry.Page.HasDraft)
   { %>
                    <img src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/draft.gif") %>" alt="<%=_Encoded("Draft") %>" title="<%=_Encoded("The page has a draft") %>" style="margin:0 0 -2px 0;" /><%=_Encoded(" Draft")%>
                    <% }
   else
   { %>
                    <%=_Encoded("No Draft")%>               
                    <% } %>
<%--Scheduled--%> 
                    <% if (!pageEntry.Page.IsPublished)
                       { %>
                        <%if (pageEntry.Page.ScheduledPublishUtc != null) { %>
                            <%=" | " %>
                            <img src="<%=ResolveUrl("~/Modules/Orchard.Pages/Content/Admin/images/scheduled.gif") %>" alt="<%=_Encoded("Scheduled") %>" title="<%=_Encoded("The page is scheduled for publishing") %>" style="margin:0 0 -2px 0;" />                            
                            <%=string.Format("Scheduled: {0:d}", pageEntry.Page.ScheduledPublishUtc.Value) %>
                       <% }%>
                    <% } %>   
                    
<%--Author--%>                
<%=_Encoded(" | By {0}", pageEntry.Page.Creator.UserName)%>
</p>
</div>
              
<div style="float:right;">
<span style="margin:0; text-align:right; font-size:1.4em;">
                <% if (pageEntry.Page.HasPublished)
                   { %>
                        <%=Html.ActionLink("View", "Item", new { controller = "Page", slug = pageEntry.Page.PublishedSlug })%>
                        <%=_Encoded("|")%>    
                    <% }
                   else
                   {%>
                        <%=""%>
                    <% } %>   


<%=Html.ActionLink(T("Edit").ToString(), "Edit", new { id = pageEntry.PageId })%>


</span>
</div>
<div style="clear:both;"></div>
</li>
            <%
pageIndex++;
} %>
</ul>
    </fieldset>
<% } %>


<div class="manage"><%=Html.ActionLink(T("Add a page").ToString(), "Create", new { }, new { @class = "button" })%></div>
