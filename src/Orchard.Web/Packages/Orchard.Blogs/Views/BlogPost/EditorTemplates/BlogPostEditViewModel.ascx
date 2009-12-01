<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPostEditViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<div class="sections">
    <div class="primary">
        <h3>Blog Post Content</h3>
        <%-- todo: (heskew) thin out the fieldsets if they become overkill --%>
        <fieldset>
            <label for="title">Title:</label>
            <span><%=Html.TextBoxFor(m => m.Title, new { id = "title", @class = "text" })%></span>
        </fieldset>
        <fieldset>
            <label class="sub" for="permalink">Permalink: <span>http://localhost/<%=Model.Blog.Slug %>/</span></label>
            <span><%=Html.TextBoxFor(m => m.Slug, new { id = "permalink", @class = "text" })%> <span> &laquo; How to write a permalink. &raquo; </span></span>
        </fieldset>
        <fieldset>
            <label for="body">Excerpt:</label>
            <span>[still needed]</span>
            <label for="body">Body:</label>
            <span><%=Html.TextAreaFor(m => m.Body, new { id = "body", @class = "html" })%></span>
        </fieldset>
    </div>
    <div class="secondary">
        <h3>Publish Settings</h3>
        <fieldset>
            <label for="Command_PublishNow"><%=Html.RadioButton("Command", "PublishNow", new { id = "Command_PublishNow" }) %> Publish Now</label>
        </fieldset>
        <fieldset>
            <label for="Command_PublishLater"><%=Html.RadioButton("Command", "PublishLater", new { id = "Command_PublishLater" }) %> Publish Later</label>
            <%=Html.EditorFor(m => m.Published) %>
        </fieldset>
        <fieldset>
            <label for="Command_SaveDraft"><%=Html.RadioButton("Command", "SaveDraft", new { id = "Command_SaveDraft" }) %> Save Draft</label></li>
        </fieldset>
        <fieldset>
            <input class="button" type="submit" name="submit.Save" value="Save"/>
        </fieldset>
    </div>
</div>