<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<RoutableEditorViewModel>" %>
<%@ Import Namespace="Orchard.ContentManagement.Extenstions"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.ContentManagement.Aspects"%>
<%@ Import Namespace="Orchard.Extensions"%>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<% Html.RegisterFootScript("jquery.slugify.js"); %>
<fieldset>
    <%=Html.LabelFor(m => m.Title) %>
    <%=Html.TextBoxFor(m => m.Title, new { @class = "large text" }) %>
</fieldset>
<fieldset class="permalink">
    <label class="sub" for="Slug"><%=_Encoded("Permalink")%><br /><span><%=Html.Encode(Request.Url.ToRootString()) %>/<%=Html.Encode(Model.RoutableAspect.ContainerPath) %></span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { @class = "text" })%></span>
</fieldset>
<% using (this.Capture("end-of-page-scripts")) { %>
<script type="text/javascript">
    $(function(){
        $("<%=String.Format("input#{0}Title", !string.IsNullOrEmpty(Model.Prefix) ? Model.Prefix + "_" : "") %>").blur(function(){
            $(this).slugify({
                target:$("<%=String.Format("input#{0}Slug", !string.IsNullOrEmpty(Model.Prefix) ? Model.Prefix + "_" : "") %>"),
                url:"<%=Url.Slugify() %>",
                contentType:"<%=Model.RoutableAspect.ContentItem.ContentType %>",<%
                var container = Model.RoutableAspect.ContentItem.As<ICommonAspect>().Container;
                if (container != null) { %>
                containerId:<%=container.ContentItem.Id %><%
                } %>
            })
        })
    })</script>
<% } %>