<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ModuleAddViewModel>" %>
<%@ Import Namespace="Orchard.Modules.ViewModels" %>
<h1><%: Html.TitleForPage(T("Install a Module").ToString()) %></h1><%
using (Html.BeginFormAntiForgeryPost(Url.Action("add", new { area = "Orchard.Modules" }), FormMethod.Post, new { enctype = "multipart/form-data" })) { %>
<%: Html.ValidationSummary()
%><fieldset>
    <label for="ModulePackage"><%:T("Module Package") %></label>
    <input type="file" id="ModulePackage" size="64" name="ModulePackage" />
</fieldset>
<button type="submit" class="button primaryAction"><%:T("Install") %></button><%
} %>