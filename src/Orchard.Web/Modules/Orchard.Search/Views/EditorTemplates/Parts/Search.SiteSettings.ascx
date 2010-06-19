<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SearchSettingsRecord>" %>
<%@ Import Namespace="Orchard.Search.Models"%>
<fieldset>
    <legend><%: T("Search")%></legend>
    <div>
        <%: Html.EditorFor(m => m.FilterCulture) %>
        <label class="forcheckbox" for="SearchSettings_FilterCulture"><%: T("Search results must be filtered with current culture")%></label>
        <%: Html.ValidationMessage("FilterCulture", "*")%>
    </div>
    <div>
        <label for="SearchSettings_SearchedFields"><%: T("Searched fields")%></label>
        <%: Html.EditorFor(m => m.SearchedFields)%>
        <%: Html.ValidationMessage("SearchedFields", "*")%> 
    </div>
</fieldset>