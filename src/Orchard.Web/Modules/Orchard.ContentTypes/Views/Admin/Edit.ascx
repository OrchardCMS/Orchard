<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentTypes.ViewModels.EditTypeViewModel>" %>
<% Html.RegisterStyle("admin.css");
%><h1><%:Html.TitleForPage(T("Edit Content Type").ToString())%></h1>
<p class="breadcrumb"><%:Html.ActionLink(T("Content Types").Text, "index") %><%:T(" &#62; ") %><%:T("Edit Content Type") %></p><%
using (Html.BeginFormAntiForgeryPost()) { %>
    <%--// todo: come up with real itemtype definitions and locations for said definitions--%>
    <div itemscope="itemscope" itemid="<%:Model.Name %>" itemtype="http://orchardproject.net/data/ContentType"><%:Html.ValidationSummary() %>
    <fieldset>
        <label for="DisplayName"><%:T("Display Name") %></label>
        <%:Html.TextBoxFor(m => m.DisplayName, new { @class = "textMedium" })%>
        <%--// todo: if we continue to go down the midrodata route, some helpers would be nice--%>
        <meta itemprop="DisplayName" content="<%:Model.DisplayName %>" /><%--
        // has unintended consequences (renamging the type) - changing the name creates a new type of that name--%>
        <meta itemprop="Name" content="<%:Model.Name %>" />
        <%:Html.HiddenFor(m => m.Name) %>
    </fieldset><%
    Html.RenderTemplates(Model.Templates); %>
    <div class="manage-type">
        <h2><%:T("Fields") %></h2>
        <div class="manage add-to-type"><%: Html.ActionLink(T("Add").Text, "AddFieldTo", new { area = "Orchard.ContentTypes", id = Model.Name }, new { @class = "button" }) %></div><%:
        Html.EditorFor(m => m.Fields, "Fields", "") %>
        <h2><%:T("Parts") %></h2>
        <div class="manage add-to-type"><%: Html.ActionLink(T("Add").Text, "AddPartsTo", new { area = "Orchard.ContentTypes", id = Model.Name }, new { @class = "button" })%></div><%:
        Html.EditorFor(m => m.Parts, "TypeParts", "") %>
    </div>
    <fieldset class="action">
        <button class="primaryAction" type="submit"><%:T("Save") %></button>
    </fieldset>
    </div><%
}
using (this.Capture("end-of-page-scripts")) { %>
<script type="text/javascript">
    (function ($) {
        $(".manage-field h3,.manage-part h3").expandoControl(function (controller) { return controller.nextAll(".details"); }, { collapse: true, remember: false });
        $(".manage-field h4").expandoControl(function (controller) { return controller.nextAll(".settings"); }, { collapse: true, remember: false });
    })(jQuery);
</script><%
} %>