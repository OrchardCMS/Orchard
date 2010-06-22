<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<EditTypePartViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
    <fieldset>
        <h3><%:Model.PartDefinition.Name %></h3>
        <div class="manage add-to-type">
        <%--// these inline forms can't be here. should probably have some JavaScript in here to build up the forms and add the "remove" link.
            // get the antiforgery token from the edit type form and mark up the part in a semantic way so I can get some info from the DOM --%>
            <%:Html.Link("[remove]", "#forshowonlyandnotintendedtowork") %>
<%--        <% using (Html.BeginFormAntiForgeryPost(Url.Action("RemovePart", new { area = "Contents" }), FormMethod.Post, new {@class = "inline link"})) { %>
            <%:Html.Hidden("name", Model.PartDefinition.Name, new { id = "" }) %>
            <button type="submit" title="<%:T("Remove") %>"><%:T("Remove") %></button>
        <% } %> --%>
        </div>
        <%:Html.EditorFor(m => m.Settings, "Settings", "") %><%
    if (Model.PartDefinition.Settings.Any()) { %>
        <h4><%:T("Tenant-wide settings") %></h4>
        <div class="manage"><%:Html.ActionLink(T("Edit part settings").Text, "EditPart", new { area = "Contents", id = Model.PartDefinition.Name }) %></div>
        <%:Html.DisplayFor(m => m.PartDefinition.Settings, "Settings", "PartDefinition") %><%
    } %>
        <%:Html.EditorFor(m => m.PartDefinition.Fields, "Part.Fields") %>
        <%:Html.Hidden("PartDefinition.Name", Model.PartDefinition.Name) %>
    </fieldset>