<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%Html.RegisterStyle("site.css");%>
<div id="wrapper">
    <div id="branding">
        <h1>
            <a href="/"><span class="displayText">Project Orchard</span></a></h1>
        <%-- todo:(nheskew) this will need to all go in the header zone (user widget) --%>
        <% Html.Include("user"); %>
    </div>
    <%--Top Navigation--%>
    <%-- todo:(nheskew) this will need to be a generated menu --%>
    <% Html.Include("menu"); %>
    <div id="content" class="clearLayout">
        <ol id="contentMain">
            <li class="moduleContentMain">
                <%--Main Content--%>
                <%Html.ZoneBody("content");%>
            </li>
        </ol>
        <%--Right sidebar--%>
        <ol id="contentSub">
            <li class="moduleSub">
                <h6>
                    Sidebar</h6>
                <p>
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas adipiscing dolor
                    vel nunc molestie laoreet. Curabitur vitae elit et massa consequat interdum. Curabitur
                    blandit leo nec magna dictum vitae mollis tellus gravida. Morbi non condimentum
                    neque. Suspendisse commodo condimentum feugiat. Class aptent taciti sociosqu ad
                    litora torquent per conubia nostra, per inceptos himenaeos.</p>
                <p>
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas adipiscing dolor
                    vel nunc molestie laoreet. Curabitur vitae elit et massa consequat interdum. Curabitur
                    blandit leo nec magna dictum vitae mollis tellus gravida. Morbi non condimentum
                    neque. Suspendisse commodo condimentum feugiat. Class aptent taciti sociosqu ad
                    litora torquent per conubia nostra, per inceptos himenaeos.</p>
                <p>
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas adipiscing dolor
                    vel nunc molestie laoreet. Curabitur vitae elit et massa consequat interdum. Curabitur
                    blandit leo nec magna dictum vitae mollis tellus gravida. Morbi non condimentum
                    neque. Suspendisse commodo condimentum feugiat. Class aptent taciti sociosqu ad
                    litora torquent per conubia nostra, per inceptos himenaeos.</p>
            </li>
        </ol>
        <div class="clearLayout">
        </div>
    </div>
    <%-- End Content --%>
    <%Html.Zone("footer");%>
    <% Html.Include("footer"); %>
</div>
