<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<FeatureListViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Modules.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Features").ToString()) %></h1>
<h2><%=T("Available Features") %></h2>
<% if (Model.Features.Count() > 0) { %>
<ul class="contentItems blogs"><%
    foreach (var featureGroup in Model.Features.OrderBy(f => f.Descriptor.Name).GroupBy(f => f.Descriptor.Category)) { %>
    <li>
        <h3><%=Html.Encode(featureGroup.First().Descriptor.Category ?? T("General")) %></h3>
        <ul><%
            foreach (var feature in featureGroup.Select(f => f.Descriptor)) {%>
            <li>
                <h4><%=Html.Encode(feature.Name) %></h4>
                <p><%=T("From: {0}", Html.Encode(feature.Extension.DisplayName)) %></p><%
                if (!string.IsNullOrEmpty(feature.Description)) { %>
                <p><%=Html.Encode(feature.Description) %></p><%
                }
                if (feature.Dependencies.Count() > 0) {%>
                <h5><%=_Encoded("Depends on:")%></h5>
                <ul><%
                    foreach (var dependency in feature.Dependencies) { %>
                    <li><%=Html.Encode(dependency) %></li><%
                    } %>
                </ul><%
                }%>
            </li><%
            } %>
        </ul>
    </li><%
    } %>
</ul><%
 } %>