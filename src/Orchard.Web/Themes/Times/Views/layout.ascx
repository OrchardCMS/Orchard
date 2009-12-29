<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BaseViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<%Html.RegisterStyle("site.css");%>

<%--Top Navigation--%>
<%-- todo:(nheskew) this will need to be a generated menu --%>
<% Html.Include("menu"); %>


<div id="wrapper">
<div id="header">
<h1>Times new roman</h1>

<%-- Welcome Jon Doe | <a href="/">Sign out</a> --%>
<%-- todo:(nheskew) this will need to all go in the header zone (user widget) --%>
<% Html.Include("user"); %>

</div>

<div id="main">
<div id="contentMain">
<ul>
<li>
<%--Main Content--%>
<%Html.ZoneBody("content");%>
</li>
<li>
<p>Paragraph - Regular<br />
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas adipiscing dolor vel nunc molestie laoreet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas adipiscing dolor vel nunc molestie laoreet. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas adipiscing dolor vel nunc molestie laoreet. Curabitur vitae elit et massa consequat interdum. Curabitur blandit leo nec magna dictum vitae mollis tellus gravida. Morbi non condimentum neque. Suspendisse commodo condimentum feugiat. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.</p>
</li>
<li>
<h1>Heading 1</h1>
<h2>Heading 2</h2>
<h3>Heading 3</h3>
<h4>Heading 4</h4>
<h5>Heading 5</h5>
<h6>Heading 6</h6>
</li>
</ul>
</div>
<div id="sideBar1">
<ul>
<li><h3>Heading</h3></li>
<li><p class="small">Paragraph - Small<br />
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas adipiscing dolor vel nunc molestie laoreet. Curabitur vitae elit et massa consequat interdum. Curabitur blandit leo nec magna dictum vitae mollis tellus gravida. Morbi non condimentum neque. Suspendisse commodo condimentum feugiat. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.</p></li>
</ul>
</div>

<%-- End Content --%>
<%Html.Zone("footer");%>
<% Html.Include("footer"); %>

</div>
</div>