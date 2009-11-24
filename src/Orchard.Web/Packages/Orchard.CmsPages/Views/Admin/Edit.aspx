<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<Orchard.CmsPages.ViewModels.PageEditViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.Models" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <h2><%=_Encoded("Edit Page") %></h2>
    <p class="bottomSpacer"><%=_Encoded("about setting up a page") %></p>
    <%=Html.ValidationSummary() %>
    <div class="sections">
        <%using (Html.BeginForm()) { %>
        <div class="primary">
            <h3><%=_Encoded("Page Content") %></h3>
            <%-- todo: (heskew) change the editors to be self-contained (fieldset > editor) --%>
            <%=Html.EditorFor(m => m.Revision.Title, "inputTextLarge") %>
            <%=Html.EditorFor(m => m.Revision.Slug, "inputTextPermalink") %>
            <% foreach (ContentItem content in Model.Revision.Contents) {
                %><fieldset>
                <label for="<%="Revision.Contents[" + content.ZoneName + "].Content" %>"><%=_Encoded("Zone Name") %>: <%= content.ZoneName %></label>
                <% if (Model.Template != null && Model.Template.Zones.Contains(content.ZoneName) == false) {
                    %><span class="warning">These contents are assigned to a zone that does not exist in the current template. Please delete it or copy it to another zone.</span><%
                   } %>
                <%= Html.TextArea("Revision.Contents[" + content.ZoneName + "].Content", content.Content) %>
            </fieldset><%
               } %>
            <fieldset>
                <p><strong>Current layout:</strong>
                    <%=Html.Encode(Model.Revision.TemplateName) %>
                    <%=Html.ActionLink("Change Template", "ChooseTemplate", new { Model.Revision.Page.Id }, new { @class = "linkButton" }) %>
                    </p>
            </fieldset>
        </div>
        <div class="secondary">
            <h3><%=_Encoded("Publish Settings") %></h3>
            <fieldset>
            <ol class="formList">
                <li><label for="Command_PublishNow"><%=Html.RadioButton("Command", "PublishNow", new { id = "Command_PublishNow" }) %> Publish Now</label></li>
                <li>
                <label for="Command_PublishLater"><%=Html.RadioButton("Command", "PublishLater", new { id = "Command_PublishLater" }) %> Publish Later</label>
                <%=Html.EditorFor(m => m.PublishLaterDate) %>
                </li>
                <li><label for="Command_SaveDraft"><%=Html.RadioButton("Command", "SaveDraft", new { id = "Command_SaveDraft" }) %> Save Draft</label></li>
                <li>
                    <input class="button" type="submit" name="submit.Save" value="Save"/>
                    <input class="button" type="submit" name="submit.DeleteDraft" value="Delete Draft" <%=Model.CanDeleteDraft ? "" : "disabled" %>/>
                </li>
            </ol>
            </fieldset>
        </div>
        <%}/*EndForm*/%>
    </div>
<% Html.Include("Foot"); %>