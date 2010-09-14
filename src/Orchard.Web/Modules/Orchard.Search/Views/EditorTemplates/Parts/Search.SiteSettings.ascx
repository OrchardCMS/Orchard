<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SearchSettingsViewModel>" %>
<%@ Import Namespace="Orchard.Search.ViewModels"%>
<fieldset>
    <legend><%: T("Search")%></legend>
    <%-- TODO: (sebros) Implementation details after Alpha
    <div>
        <%: Html.EditorFor(m => m.FilterCulture) %>
        <label class="forcheckbox" for="SearchSettings_FilterCulture"><%: T("Search results must be filtered with current culture")%></label>
        <%: Html.ValidationMessage("FilterCulture", "*")%>
    </div>
    --%>
    <div>
        <%
            for(var i=0; i<Model.Entries.Count; i++) {
                var j = i;%>
            <input type="checkbox" value="true" <% if(Model.Entries[j].Selected ) { %> checked="checked" <% } %>
            name="<%:Html.FieldNameFor(m => m.Entries[j].Selected)%>" id="<%:Html.FieldIdFor(m => m.Entries[j].Selected)%>"/> 
            <%: Html.HiddenFor(m => m.Entries[j].Field)%>
            <label class="forcheckbox" for="<%:Html.FieldIdFor(m => m.Entries[j].Selected)%>"><%: Model.Entries[j].Field%></label>
        <%
           }%>
    </div>
</fieldset>