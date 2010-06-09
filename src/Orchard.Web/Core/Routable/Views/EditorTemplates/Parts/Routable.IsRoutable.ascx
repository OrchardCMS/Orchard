<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Routable.ViewModels.RoutableEditorViewModel>" %>
<%@ Import Namespace="Orchard.Utility.Extensions"%>
<%@ Import Namespace="Orchard.ContentManagement.Extenstions"%>

<% Html.RegisterFootScript("jquery.slugify.js"); %>
<fieldset>
    <%=Html.LabelFor(m => m.Title) %>
    <%=Html.TextBoxFor(m => m.Title, new { @class = "large text" }) %>
</fieldset>
<fieldset class="permalink">
    <label class="sub" for="Slug"><%=_Encoded("Permalink")%><br /><span><%=Html.Encode(Request.ToRootUrlString())%>/<%:Model.DisplayLeadingPath %></span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { @class = "text" })%></span>
</fieldset>


<% using (this.Capture("end-of-page-scripts")) { %>
<script type="text/javascript">
    $(function(){
        //pull slug input from tab order
        $("#<%:Html.FieldIdFor(m=>m.Slug)%>").attr("tabindex",-1);
        $("#<%:Html.FieldIdFor(m=>m.Title)%>").blur(function(){
            $(this).slugify({
                target:$("#<%:Html.FieldIdFor(m=>m.Slug)%>"),
                url:"<%=Url.Action("Slugify","Item",new RouteValueDictionary{{"Area","Routable"}})%>",
                contentType:"<%=Model.ContentType %>",
                id:"<%=Model.Id %>" <%if (Model.ContainerId != null) { %>,
                containerId:<%=Model.ContainerId %><%} %>
            })
        })
    })</script>
<% } %>
