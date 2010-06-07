<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<RoutableEditorViewModel>" %>
<%@ Import Namespace="Orchard.Utility.Extensions"%>
<%@ Import Namespace="Orchard.ContentManagement.Extenstions"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.ContentManagement.Aspects"%>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<% Html.RegisterFootScript("jquery.slugify.js"); %>
<fieldset>
    <%=Html.LabelFor(m => m.Title) %>
    <%=Html.TextBoxFor(m => m.Title, new { @class = "large text" }) %>
</fieldset>
<fieldset class="permalink">
    <label class="sub" for="Slug"><%=_Encoded("Permalink")%><br /><span><%=Html.Encode(Request.ToRootUrlString())%>/<%=Html.Encode(Model.RoutableAspect.ContentItemBasePath) %></span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { @class = "text" })%></span>
</fieldset>
<% using (this.Capture("end-of-page-scripts")) { %>
<script type="text/javascript">
    $(function(){
        //pull slug input from tab order
        $("<%=String.Format("input#{0}Slug", !string.IsNullOrEmpty(Model.Prefix) ? Model.Prefix + "_" : "") %>").attr("tabindex",-1);
        $("<%=String.Format("input#{0}Title", !string.IsNullOrEmpty(Model.Prefix) ? Model.Prefix + "_" : "") %>").blur(function(){
            $(this).slugify({
                target:$("<%=String.Format("input#{0}Slug", !string.IsNullOrEmpty(Model.Prefix) ? Model.Prefix + "_" : "") %>"),
                url:"<%=Url.Slugify() %>",
                contentType:"<%=Model.RoutableAspect.ContentItem.ContentType %>",
                id:"<%=Model.RoutableAspect.ContentItem.Id %>"<%
                var commonAspect = Model.RoutableAspect.ContentItem.As<ICommonAspect>();
                var container = commonAspect != null ? commonAspect.Container : null;
                if (container != null) { %>,
                containerId:<%=container.ContentItem.Id %><%
                } %>
            })
        })
    })</script>
<% } %>