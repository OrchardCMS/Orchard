<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Packaging.ViewModels.PackagingSourcesViewModel>" %>
<h1>
    <%: Html.TitleForPage(T("Gallery Feeds").ToString())%></h1>

<div class="manage">
    <%:Html.ActionLink(T("Add a Feed").Text, "AddSource", new { }, new { @class = "button primaryAction" }) %>
</div>

	<fieldset>
	    <table class="items" summary="<%: T("This is a table of the gallery feeds in your application") %>">
		    <colgroup>
			    <col id="Col1" />
			    <col id="Col2" />
			    <col id="Col3" />
		    </colgroup>
		    <thead>
			    <tr>
				    <th scope="col"><%: T("Title")%></th>
				    <th scope="col"><%: T("Url")%></th>
				    <th scope="col"></th>
			    </tr>
		    </thead>
            <%
                foreach ( var item in Model.Sources ) {
            %>
            <tr>
                <td>
                    <%: item.FeedTitle %>
                </td>
                <td>
                    <%:Html.Link(item.FeedUrl, item.FeedUrl)%>
                </td>
                <td>
                    <%: Html.ActionLink(T("Remove").ToString(), "Remove", new { id = item.Id })%>
                </td>
            </tr>
            <% } %>
        </table>
    </fieldset>
