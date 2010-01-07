<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<fieldset>
    <legend><%=_Encoded("Publish Settings")%></legend>
    <label for="Command_SaveDraft"><%=Html.RadioButton("Command", "SaveDraft", true, new { id = "Command_SaveDraft" }) %> <%=_Encoded("Save Draft")%></label>
</fieldset>