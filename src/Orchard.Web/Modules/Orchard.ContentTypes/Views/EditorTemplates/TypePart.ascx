<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.ContentTypes.ViewModels.EditTypePartViewModel>" %>
    <fieldset class="manage-part" itemscope itemid="<%:Model.PartDefinition.Name %>" itemtype="http://orchardproject.net/data/ContentTypePart">
        <h3 itemprop="Name"><%:Model.PartDefinition.Name %></h3>
        <div class="manage">
            <%:Html.ActionLink(T("Remove").Text, "RemovePartFrom", new { area = "Orchard.ContentTypes", id = Model.Type.Name, Model.PartDefinition.Name }, new { itemprop = "RemoveUrl UnsafeUrl" })%><%--// <- some experimentation--%>
        </div><%
        if (Model.Templates.Any()) { %>
        <div class="settings"><%
        Html.RenderTemplates(Model.Templates); %>
        </div><%
        } %>
        <div class="manage minor"><%:Html.ActionLink(T("Edit global part config").Text, "EditPart", new { area = "Orchard.ContentTypes", id = Model.PartDefinition.Name })%></div>
        <%:Html.DisplayFor(m => m.PartDefinition.Settings, "Settings", "PartDefinition")
        %><%:Html.EditorFor(m => m.PartDefinition.Fields, "TypePartFields", "PartDefinition")
        %><%:Html.Hidden("PartDefinition.Name", Model.PartDefinition.Name) %>
    </fieldset>