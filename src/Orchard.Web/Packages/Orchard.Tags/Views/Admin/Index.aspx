<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<TagsAdminIndexViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h2><%=Html.TitleForPage("Manage Tags")%></h2>
<% using(Html.BeginFormAntiForgeryPost()) { %>
	<%=Html.ValidationSummary() %>
    <fieldset class="actions bulk">
        <label for="publishActions">Actions: </label>
        <select id="publishActions" name="<%=Html.NameOf(m => m.BulkAction)%>">
            <%=Html.SelectOption(Model.BulkAction, TagAdminIndexBulkAction.None, "Choose action...")%>
            <%=Html.SelectOption(Model.BulkAction, TagAdminIndexBulkAction.Delete, "Delete")%>
        </select>
        <input class="button" type="submit" name="submit" value="Apply" />
    </fieldset>
    <div class="manage"><%=Html.ActionLink("Add a tag", "Create", new { }, new { @class = "button" })%></div>
	<fieldset>
	    <table class="items" summary="This is a table of the tags in your application">
		    <colgroup>
			    <col id="Col1" />
			    <col id="Col2" />
			    <col id="Col3" />
		    </colgroup>
		    <thead>
			    <tr>
				    <th scope="col">&nbsp;&darr;<%-- todo: (heskew) something more appropriate for "this applies to the bulk actions --%></th>
				    <th scope="col">Name</th>
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
                    <input type="hidden" value="<%=Model.Tags[tagIndex].Tag.Id%>" name="<%=Html.NameOf(m => m.Tags[ti].Tag.Id)%>"/>
                    <input type="checkbox" value="true" name="<%=Html.NameOf(m => m.Tags[ti].IsChecked)%>"/>
                </td>
                <td>
                    <%=Html.ActionLink(tagEntry.Tag.TagName, "Search", new {id = tagEntry.Tag.Id}) %>
                </td>
                <td>
                <%=Html.ActionLink("Edit", "Edit", new {id = tagEntry.Tag.Id}) %>
                </td>
            </tr>
            <% tagIndex++; } %>
        </table>
    </fieldset>
    <div class="manage"><%=Html.ActionLink("Add a tag", "Create", new { }, new { @class = "button" })%></div>
<% } %>