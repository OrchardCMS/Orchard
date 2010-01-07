<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<CommentSettingsRecord>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<fieldset>
    <legend><%=_Encoded("Comments")%></legend>
    <fieldset>
        <%=Html.EditorFor(m => m.EnableCommentsOnPages) %>
        <label class="forcheckbox" for="CommentSettings_EnableCommentsOnPages"><%=_Encoded("Enable comments on pages")%></label>
        <%=Html.ValidationMessage("EnableCommentsOnPages", "*")%>
    </fieldset>
    <fieldset>
        <%=Html.EditorFor(m => m.EnableCommentsOnPosts) %>
        <label class="forcheckbox" for="CommentSettings_EnableCommentsOnPosts"><%=_Encoded("Enable comments on blog posts")%></label>
        <%=Html.ValidationMessage("EnableCommentsOnPosts", "*")%>
    </fieldset>
    <fieldset>
        <%=Html.EditorFor(m => m.RequireLoginToAddComment) %>
        <label class="forcheckbox" for="CommentSettings_RequireLoginToAddComment"><%=_Encoded("Require login to comment")%></label>
        <%=Html.ValidationMessage("RequireLoginToAddComment", "*")%>
    </fieldset>
    <fieldset>
        <%=Html.EditorFor(m => m.EnableSpamProtection) %>
        <label class="forcheckbox" for="CommentSettings_EnableSpamProtection"><%=_Encoded("Enable spam protection") %></label>
        <%=Html.ValidationMessage("EnableSpamProtection", "*")%>
    </fieldset>
    <fieldset>
        <label for="CommentSettings_AkismetKey"><%=_Encoded("Akismet key") %></label>
        <%=Html.EditorFor(m => m.AkismetKey) %>
        <%=Html.ValidationMessage("AkismetKey", "*")%>
    </fieldset>
    <fieldset>
        <label for="CommentSettings_AkismetUrl"><%=_Encoded("Akismet endpoint URL") %></label>
        <%=Html.EditorFor(m => m.AkismetUrl) %>
        <%=Html.ValidationMessage("AkismetUrl", "*")%>
    </fieldset>
</fieldset>