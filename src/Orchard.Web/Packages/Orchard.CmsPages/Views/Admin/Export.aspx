<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Orchard.CmsPages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h2>Export</h2>
<p>Possible text about setting up a page goes here. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla erat turpis, blandit eget feugiat nec, tempus vel quam. Mauris et neque eget justo suscipit blandit.</p>
    <ol>
        <% foreach (var page in Model) {
            %><li>Page Id <%=page.Id %> 
            <ol>
                <% foreach (var revision in page.Revisions) {
                   %><li>Revision <%=revision.Number %> <strong><%=revision.Title %></strong> ~/<%=revision.Slug %>
                    <br />Modified: <%=revision.ModifiedDate %> 
                    <br />Published: <%=revision.PublishedDate %></li><%
                   } %>
            </ol>
        </li><%
           } %>
    </ol>