<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SetupViewModel>" %>
<%@ Import Namespace="Orchard.Core.Setup.ViewModels"%>
<h1><%=Html.TitleForPage("Orchard " + _Encoded("Setup"))%></h1><%
using (Html.BeginFormAntiForgeryPost()) { %>
<%=Html.ValidationSummary() %>
<fieldset>
    <%=Html.LabelFor(svm => svm.SiteName) %>
    <%=Html.EditorFor(svm => svm.SiteName) %>
    <%=Html.ValidationMessage("SiteName", "*") %>
</fieldset>
<fieldset>
    <%=Html.LabelFor(svm => svm.AdminUsername) %>
    <%=Html.EditorFor(svm => svm.AdminUsername)%>
    <%=Html.ValidationMessage("AdminUsername", "*")%>
</fieldset>
<fieldset>
    <%=Html.LabelFor(svm => svm.AdminPassword) %>
    <%=Html.EditorFor(svm => svm.AdminPassword) %>
    <%=Html.ValidationMessage("AdminPassword", "*") %>
</fieldset>
<fieldset>
    <%=Html.ValidationMessage("DatabaseOptions", "Unable to setup data storage") %>
    <div>
        <input type="radio" name="databaseOption" id="builtin" value="true" checked="checked" />
        <label for="builtin"><%=_Encoded("Use built-in data storage (SQL Lite)") %></label>
    </div>
    <div>
        <input type="radio" name="databaseOption" id="sql" value="false" />
        <label for="sql"><%=_Encoded("Use an existing SQL Server (or SQL Express) database") %></label>
        <!-- Should add some javascript to hide the connection string field if that option isn't selected -->
        <label for="connection"><%=_Encoded("Connection string") %></label>
        <%=Html.EditorFor(svm => svm.DatabaseConnectionString)%>
    </div>
</fieldset>
<fieldset>
    <input class="button" type="submit" value="<%=_Encoded("Finish Setup") %>" />
</fieldset><%
} %>