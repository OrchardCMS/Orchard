<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<div class="sections">
    <div class="primary">
        <%-- todo: (heskew) thin out the fieldsets if they become overkill --%>
        <fieldset>
            <label for="title">Title</label>
            <span><%=Html.TextBoxFor(m => m.Title, new { id = "title", @class = "large text" })%></span>
        </fieldset>
        <fieldset>
            <label class="sub" for="permalink">Permalink<br /><span><%=Request.Url.ToRootString() %>/<%=Model.Blog.Slug %>/</span></label>
            <span><%=Html.TextBoxFor(m => m.Slug, new { id = "permalink", @class = "text" })%></span>
        </fieldset>
        <fieldset>
            <%--<label for="body">Excerpt</label>
            <span>[still needed]</span>--%>
            <label for="body">Body</label>
            <span><%=Html.TextAreaFor(m => m.Body, new { id = "body", @class = "html" })%></span>
        </fieldset>
    </div>
    <div class="secondary">
        <fieldset>
            <legend>Publish Settings</legend>
            <label for="Command_SaveDraft"><%=Html.RadioButton("Command", "SaveDraft", true, new { id = "Command_SaveDraft" }) %> Save Draft</label><br />
            <input class="button" type="submit" name="submit.Save" value="Save"/>
        </fieldset>
        <%--<fieldset>
            <label for="Command_PublishNow"><%=Html.RadioButton("Command", "PublishNow", new { id = "Command_PublishNow" }) %> Publish Now</label>
        </fieldset>
        <fieldset>
            <label for="Command_PublishLater"><%=Html.RadioButton("Command", "PublishLater", new { id = "Command_PublishLater" }) %> Publish Later</label>
            <%=Html.EditorFor(m => m.Published) %>
        </fieldset>--%>

    </div>
</div>