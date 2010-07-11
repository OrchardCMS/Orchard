<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<CommentSettingsRecord>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<fieldset>
    <legend><%: T("Comments")%></legend>
    <div>
        <%: Html.EditorFor(m => m.ModerateComments) %>
        <label class="forcheckbox" for="CommentSettings_ModerateComments"><%: T("Comments must be approved before they appear")%></label>
        <%: Html.ValidationMessage("ModerateComments", "*")%>
    </div>
    <div>
        <%: Html.EditorFor(m => m.EnableSpamProtection) %>
        <label class="forcheckbox" for="CommentSettings_EnableSpamProtection"><%: T("Enable spam protection") %></label>
        <%: Html.ValidationMessage("EnableSpamProtection", "*")%>
    </div>
    <div data-controllerid="CommentSettings_EnableSpamProtection">
        <label for="CommentSettings_AkismetKey"><%: T("Akismet key") %></label>
        <%: Html.EditorFor(m => m.AkismetKey) %>
        <%: Html.ValidationMessage("AkismetKey", "*")%>
    </div>
    <div data-controllerid="CommentSettings_EnableSpamProtection">
        <label for="CommentSettings_AkismetUrl"><%: T("Akismet endpoint URL") %></label>
        <%: Html.EditorFor(m => m.AkismetUrl) %>
        <%: Html.ValidationMessage("AkismetUrl", "*")%>
    </div>
</fieldset>