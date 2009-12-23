<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<Orchard.CmsPages.ViewModels.PageEditViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.Models" %>
<h2><%=Html.TitleForPage("Edit Page") %></h2>
<p><%=_Encoded("about setting up a page") %></p>
<%=Html.ValidationSummary() %>
<div class="sections">
    <% using (Html.BeginFormAntiForgeryPost()) { %>
    <div class="primary">
        <h3><%=_Encoded("Page Content") %></h3>
        <%-- todo: (heskew) change the editors to be self-contained (fieldset > editor) --%>
        <%=Html.EditorFor(m => m.Revision.Title, "inputTextLarge") %>
        <%=Html.EditorFor(m => m.Revision.Slug, "inputTextPermalink") %>
        <% foreach (ContentItem content in Model.Revision.Contents) {
            %><fieldset>
            <label for="<%="Revision.Contents[" + content.ZoneName + "].Content" %>"><%=_Encoded("Zone Name") %>: <%= content.ZoneName %></label>
            <% if (Model.Template != null && Model.Template.Zones.Contains(content.ZoneName) == false) {
                %><span class="warning message">These contents are assigned to a zone that does not exist in the current template. Please delete it or copy it to another zone.</span><%
               } %>
            <%= Html.TextArea("Revision.Contents[" + content.ZoneName + "].Content", content.Content, new { @class = "html" }) %>
        </fieldset><%
           } %>
        <fieldset>
            <p><strong>Current layout:</strong>
                <%=Html.Encode(Model.Revision.TemplateName) %>
                <%=Html.ActionLink("Change Template", "ChooseTemplate", new { Model.Revision.Page.Id }, new { @class = "button" }) %>
                </p>
        </fieldset>
    </div>
    <div class="secondary">
        <h3><%=_Encoded("Publish Settings") %></h3>
        <fieldset>
            <label for="Command_PublishNow"><%=Html.RadioButton("Command", "PublishNow", new { id = "Command_PublishNow" }) %> Publish Now</label>
        </fieldset>
        <fieldset>
            <label for="Command_PublishLater"><%=Html.RadioButton("Command", "PublishLater", new { id = "Command_PublishLater" }) %> Publish Later</label>
            <%=Html.EditorFor(m => m.PublishLaterDate) %>
        </fieldset>
        <fieldset>
            <label for="Command_SaveDraft"><%=Html.RadioButton("Command", "SaveDraft", new { id = "Command_SaveDraft" }) %> Save Draft</label>
        </fieldset>
        <fieldset>
            <input class="button" type="submit" name="submit.Save" value="Save"/>
            <input class="delete button" type="submit" name="submit.DeleteDraft" value="Delete Draft" <%=Model.CanDeleteDraft ? "" : "disabled" %>/>
        </fieldset>
    </div>
    <% } %>
</div>