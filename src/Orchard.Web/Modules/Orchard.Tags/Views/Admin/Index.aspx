<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<TagsAdminIndexViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h1><%: Html.TitleForPage(T("Manage Tags").ToString())%></h1>
<% using(Html.BeginFormAntiForgeryPost()) { %>
	<%: Html.ValidationSummary() %>
    <fieldset class="actions bulk">
        <label for="publishActions"><%: T("Actions:") %></label>
        <select id="publishActions" name="<%: Html.NameOf(m => m.BulkAction)%>">
            <%: Html.SelectOption(Model.BulkAction, TagAdminIndexBulkAction.None, T("Choose action...").ToString())%>
            <%: Html.SelectOption(Model.BulkAction, TagAdminIndexBulkAction.Delete, T("Remove").ToString())%>
        </select>
        <input class="button" type="submit" name="submit.BulkEdit" value="<%: T("Apply") %>" />
    </fieldset>
    <div class="manage"><%: Html.ActionLink(T("Add a tag").ToString(), "Create", new { }, new { @class = "button primaryAction" })%></div>
	<fieldset>
	    <table class="items" summary="<%: T("This is a table of the tags in your application") %>">
		    <colgroup>
			    <col id="Col1" />
			    <col id="Col2" />
			    <col id="Col3" />
		    </colgroup>
		    <thead>
			    <tr>
				    <th scope="col">&nbsp;&darr;<%-- todo: (heskew) something more appropriate for "this applies to the bulk actions --%></th>
				    <th scope="col"><%: T("Name")%></th>
				    <th scope="col"></th>
			    </tr>
		    </thead>
            <%
                int tagIndex = 0;
                foreach (var tagEntry in Model.Tags) {
                    var ti = tagIndex;
            %>
            <tr>
                <td>
                    <input type="hidden" value="<%=Model.Tags[tagIndex].Tag.Id%>" name="<%: Html.NameOf(m => m.Tags[ti].Tag.Id)%>"/>
                    <input type="checkbox" value="true" name="<%: Html.NameOf(m => m.Tags[ti].IsChecked)%>"/>
                </td>
                <td>
                    <%: Html.ActionLink(Html.Encode(tagEntry.Tag.TagName), "Search", new {id = tagEntry.Tag.Id}) %>
                </td>
                <td>
                <%: Html.ActionLink(T("Edit").ToString(), "Edit", new {id = tagEntry.Tag.Id}) %>
                </td>
            </tr>
            <% tagIndex++; } %>
        </table>
    </fieldset>
    <div class="manage"><%: Html.ActionLink(T("Add a tag").ToString(), "Create", new { }, new { @class = "button primaryAction" })%></div>
<% } %>