<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Routable.ViewModels.RoutableEditorViewModel>" %>
<%@ Import Namespace="Orchard.Utility.Extensions"%>

<% Html.RegisterFootScript("jquery.slugify.js"); %>
<fieldset>
    <%: Html.LabelFor(m => m.Title) %>
    <%: Html.TextBoxFor(m => m.Title, new { @class = "large text" }) %>
</fieldset>
<fieldset class="permalink">
    <label class="sub" for="Slug"><%: T("Permalink")%><br /><span><%: Request.ToRootUrlString()%>/<%: Model.DisplayLeadingPath %></span></label>
    <span><%: Html.TextBoxFor(m => m.Slug, new { @class = "text" })%></span>
    <%: Html.EditorFor(m => m.PromoteToHomePage) %>
    <label for="<%:ViewData.TemplateInfo.GetFullHtmlFieldId("PromoteToHomePage") %>" class="forcheckbox"><%: T("Set as home page") %></label>
</fieldset>
<% using (this.Capture("end-of-page-scripts")) { %>
<script type="text/javascript">
    $(function(){
        //pull slug input from tab order
        $("#<%: Html.FieldIdFor(m=>m.Slug)%>").attr("tabindex",-1);
        $("#<%: Html.FieldIdFor(m=>m.Title)%>").blur(function(){
            var slug = $("#<%:Html.FieldIdFor(m=>m.Slug)%>");
            if (slug.val()) { return true; }
            $(this).slugify({
                target:slug,
                url:"<%: Url.Action("Slugify","Item",new RouteValueDictionary{{"Area","Routable"}})%>",
                contentType:"<%: Model.ContentType %>",
                id:"<%=Model.Id %>" <%if (Model.ContainerId != null) { %>,
                containerId:<%: Model.ContainerId %><%} %>
            })
        })
    })</script>
<% } %>