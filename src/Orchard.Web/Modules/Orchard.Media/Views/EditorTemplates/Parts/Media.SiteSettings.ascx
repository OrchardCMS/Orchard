<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<MediaSettingsRecord>" %>
<%@ Import Namespace="Orchard.Media.Models"%>
<fieldset>
    <legend><%=_Encoded("Media")%></legend>
    <div>
        <label for="MediaSettings_RootMediaFolder">Media folder</label>
        <%=Html.EditorFor(x=>x.RootMediaFolder) %>
        <%=Html.ValidationMessage("RootMediaFolder", "*")%>
    </div>
</fieldset>