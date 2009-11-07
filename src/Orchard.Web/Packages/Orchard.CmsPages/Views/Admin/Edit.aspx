<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.CmsPages.ViewModels.PageEditViewModel>" %>
<%@ Import Namespace="Orchard.CmsPages.Models"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<!DOCTYPE html>
<html>
<head id="Head1" runat="server">
    <title>Index2</title>
    <% Html.Include("Head"); %>
    <script type="text/javascript" src="<%=ResolveUrl("~/Packages/TinyMce/Scripts/tiny_mce.js") %>"></script>
    <script type="text/javascript">
        tinyMCE.init({ 
            theme: "advanced",
            mode: "textareas",
            plugins: "fullscreen,autoresize",
            theme_advanced_toolbar_location: "top",
            theme_advanced_toolbar_align : "left",
	        theme_advanced_buttons3_add : "fullscreen"
        });</script>
</head>
<body>
    <% Html.Include("Header"); %>
    <div class="yui-g">
        <h2>
            Edit Page</h2>
        <p class="bottomSpacer">
            Possible text about setting up a page goes here. Lorem ipsum dolor sit amet, consectetur
            adipiscing elit. Nulla erat turpis, blandit eget feugiat nec, tempus vel quam. Mauris
            et neque eget justo suscipit blandit.</p>
        <%=Html.ValidationSummary() %>
    </div>
    <div class="yui-gc">
        <%using (Html.BeginForm()) {%>
        <div class="yui-u first">
            <h3>
                Page Content</h3>
            <ol>
                <%=Html.EditorFor(m => m.Revision.Title, "inputTextLarge")%>
                <%=Html.EditorFor(m => m.Revision.Slug, "inputTextPermalink")%>
                
                <%foreach (ContentItem content in Model.Revision.Contents) {%>
                <label for="<%="Revision.Contents[" + content.ZoneName + "].Content" %>">
                Zone Name: <%= content.ZoneName %></label>
                <%if (Model.Template != null && Model.Template.Zones.Contains(content.ZoneName) == false) {%>
                <div class="warning">These contents are assigned to a zone that does not exist in the current template. Please delete it or copy it to another zone.</div>
                <%}%>
                <%= Html.TextArea("Revision.Contents[" + content.ZoneName + "].Content", content.Content) %>
                </li>
                <%}%>
                <li>
                    <%--<%=Html.LabelFor(m=>m.Revision.TemplateName) %>
                    <%=Html.DisplayFor(m=>m.Revision.TemplateName)%>--%>
                    <p>
                        <strong>Current layout:</strong>
                        <%=Html.Encode(Model.Revision.TemplateName)%>
                        <%=Html.ActionLink("Change Template", "ChooseTemplate", new { Model.Revision.Page.Id }, new { @class = "linkButton" })%>
                    </p>
                </li>
            </ol>
        </div>
        <div class="yui-u sideBar">
            <h3>Publish Settings</h3>
            <fieldset>
            <ol class="formList">
                <li><label for="Command_PublishNow"><%=Html.RadioButton("Command", "PublishNow", new { id = "Command_PublishNow" })%> Publish Now</label></li>
                <li>
                <label for="Command_PublishLater"><%=Html.RadioButton("Command", "PublishLater", new { id = "Command_PublishLater" })%> Publish Later</label>
                <%=Html.EditorFor(m => m.PublishLaterDate)%>
                </li>
                <li><label for="Command_SaveDraft"><%=Html.RadioButton("Command", "SaveDraft", new { id = "Command_SaveDraft" })%> Save Draft</label></li>
                <li>
                    <input class="button" type="submit" name="submit.Save" value="Save"/>
                    <input class="button" type="submit" name="submit.DeleteDraft" value="Delete Draft" <%=Model.CanDeleteDraft ? "" : "disabled" %>/>
                </li>
            </ol>
            </fieldset>
        </div>
        <%}/*EndForm*/%>
    </div>
    <% Html.Include("Footer"); %>
</body>
</html>

