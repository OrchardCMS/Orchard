<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<FeaturesViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Modules.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Features").ToString()) %></h1>
<% if (Model.Features.Count() > 0) { %>
<ul class="contentItems"><%
    var featureGroups = Model.Features.OrderBy(f => f.Descriptor.Category).GroupBy(f => f.Descriptor.Category);
    foreach (var featureGroup in featureGroups) { %>
    <li<%=featureGroup == featureGroups.Last() ? " class=\"last\"" : "" %>>
        <h2><%=Html.Encode(featureGroup.First().Descriptor.Category ?? T("Uncategorized")) %></h2>
        <ul><%
            var features = featureGroup.OrderBy(f => f.Descriptor.Name);
            foreach (var feature in features) {%>
            <li<%=feature == features.Last() ? " class=\"last\"" : "" %> id="<%=Html.Encode(feature.Descriptor.Name) %>">
                <div class="summary">
                    <div class="properties">
                        <h3><%=Html.Encode(feature.Descriptor.Name) %></h3>
                        <ul class="pageStatus">
                            <li><%
                            //enabled or not
                            if (feature.IsEnabled) { %>
                                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Modules/Content/Admin/images/enabled.gif") %>" alt="<%=_Encoded("Enabled") %>" title="<%=_Encoded("This feature is currently enabled") %>" /><%=_Encoded("Enabled") %><%
                            }
                            else { %>
                                <img class="icon" src="<%=ResolveUrl("~/Modules/Orchard.Modules/Content/Admin/images/disabled.gif") %>" alt="<%=_Encoded("Disabled") %>" title="<%=_Encoded("This feature is currently disabled") %>" /><%=_Encoded("Disabled")%><%
                            } %>
                            </li><%
                            //dependencies
                            if (feature.Descriptor.Dependencies != null && feature.Descriptor.Dependencies.Count() > 0) { %>
                            <li>&nbsp;&#124;&nbsp;<%=T("Depends on: {0}", string.Join(", ", feature.Descriptor.Dependencies.Select(s => Html.Link(Html.Encode(s), string.Format("{0}#{1}", Url.Action("features", new { area = "Orchard.Modules" }), Html.Encode(s)))).OrderBy(s => s).ToArray())) %></li><%
                            } %>
                        </ul>
                    </div>
                    <div class="related"><%
                        if (feature.IsEnabled) {
                        using (Html.BeginFormAntiForgeryPost(string.Format("{0}#{1}", Url.Action("Disable", new { area = "Orchard.Modules" }), Html.AttributeEncode(feature.Descriptor.Name)), FormMethod.Post, new {@class = "inline link"})) { %>
                            <%=Html.Hidden("featureName", feature.Descriptor.Name) %>
                            <button type="submit"><%=_Encoded("Disable") %></button><%
                        }
                        } else {
                        using (Html.BeginFormAntiForgeryPost(string.Format("{0}#{1}", Url.Action("Enable", new { area = "Orchard.Modules" }), Html.AttributeEncode(feature.Descriptor.Name)), FormMethod.Post, new {@class = "inline link"})) { %>
                            <%=Html.Hidden("featureName", feature.Descriptor.Name) %>
                            <button type="submit"><%=_Encoded("Enable") %></button><%
                        }
                        } %>
                    </div>
                </div>
            </li><%
            } %>
        </ul>
    </li><%
    } %>
</ul><%
} %>   