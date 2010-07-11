<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentTypeDefinition.Part>" %>
<%@ Import Namespace="Orchard.ContentManagement.MetaData.Models" %>
    <fieldset>
        <h3><%:Model.PartDefinition.Name %></h3>
        <div class="manage add-to-type">
        <%--// these inline forms can't be here. should probably have some JavaScript in here to build up the forms and add the "remove" link.
            // get the antiforgery token from the edit type form and mark up the part in a semantic way so I can get some info from the DOM --%>
        <% using (Html.BeginFormAntiForgeryPost(Url.Action("RemovePart", new { area = "Contents" }), FormMethod.Post, new {@class = "inline link"})) { %>
            <%=Html.Hidden("name", Model.PartDefinition.Name, new { id = "" }) %>
            <button type="submit" title="<%:T("Remove") %>"><%:T("Remove") %></button>
        <% } %>
        </div>
    </fieldset>