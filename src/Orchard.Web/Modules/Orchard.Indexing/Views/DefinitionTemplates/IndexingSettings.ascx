<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Indexing.Settings.IndexingSettings>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<fieldset>
    <%:Html.EditorFor(m=>m.IncludeInIndex) %>
    <label for="<%:Html.FieldIdFor(m => m.IncludeInIndex) %>" class="forcheckbox"><%:T("Include in the index") %></label>
    <%:Html.ValidationMessageFor(m => m.IncludeInIndex)%>
</fieldset>
