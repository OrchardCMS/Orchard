<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<SetupViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Setup.ViewModels"%>
<h1><%=Html.TitleForPage(_Encoded("Get Started").ToHtmlString())%></h1>
<%
using (Html.BeginFormAntiForgeryPost()) { %>
<%=Html.ValidationSummary() %>
<h2><%=_Encoded("Please answer a few questions to configure your site.")%></h2>
<fieldset class="site">
    <div>
        <label for="SiteName"><%=_Encoded("What is the name of your site?") %></label>
        <%=Html.EditorFor(svm => svm.SiteName) %>
    </div>
    <div>
        <label for="AdminUsername"><%=_Encoded("Choose a user name:") %></label>
        <%=Html.EditorFor(svm => svm.AdminUsername)%>
    </div>
    <div>
        <label for="AdminPassword"><%=_Encoded("Choose a password:") %></label>
        <%=Html.PasswordFor(svm => svm.AdminPassword) %>
    </div>
</fieldset>
<fieldset class="data">
    <legend><%=_Encoded("How would you like to store your data?") %></legend>
    <%=Html.ValidationMessage("DatabaseOptions", "Unable to setup data storage") %>
    <div>
        <%=Html.RadioButtonFor(svm => svm.DatabaseOptions, true, new { id = "builtin" })%>
        <label for="builtin" class="forcheckbox"><%=_Encoded("Use built-in data storage (SQLite)") %></label>
    </div>
    <div>
        <%=Html.RadioButtonFor(svm => svm.DatabaseOptions, false, new { id = "sql" })%>
        <label for="sql" class="forcheckbox"><%=_Encoded("Use an existing SQL Server (or SQL Express) database") %></label>
        <span data-controllerid="sql">
            <label for="DatabaseConnectionString"><%=_Encoded("Connection string") %></label>
            <%=Html.EditorFor(svm => svm.DatabaseConnectionString)%>
            <span class="hint"><%=_Encoded("Example:") %><br /><%=_Encoded("Data Source=sqlServerName;Initial Catalog=dbName;Persist Security Info=True;User ID=userName;Password=password") %></span>
        </span>
    </div>
</fieldset>
<fieldset>
    <input class="button" type="submit" value="<%=_Encoded("Finish Setup") %>" />
</fieldset><%
} %>