<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<CommentSettingsRecord>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<fieldset>
    <legend><%=_Encoded("Comments")%></legend>
    <div>
        <%=Html.EditorFor(m => m.RequireLoginToAddComment) %>
        <label class="forcheckbox" for="CommentSettings_RequireLoginToAddComment"><%=_Encoded("Require login to comment")%></label>
        <%=Html.ValidationMessage("RequireLoginToAddComment", "*")%>
    </div>
    <div>
        <%=Html.EditorFor(m => m.EnableSpamProtection) %>
        <label class="forcheckbox" for="CommentSettings_EnableSpamProtection"><%=_Encoded("Enable spam protection") %></label>
        <%=Html.ValidationMessage("EnableSpamProtection", "*")%>
    </div>
    <div>
        <label for="CommentSettings_AkismetKey"><%=_Encoded("Akismet key") %></label>
        <%=Html.EditorFor(m => m.AkismetKey) %>
        <%=Html.ValidationMessage("AkismetKey", "*")%>
    </div>
    <div>
        <label for="CommentSettings_AkismetUrl"><%=_Encoded("Akismet endpoint URL") %></label>
        <%=Html.EditorFor(m => m.AkismetUrl) %>
        <%=Html.ValidationMessage("AkismetUrl", "*")%>
    </div>
</fieldset>