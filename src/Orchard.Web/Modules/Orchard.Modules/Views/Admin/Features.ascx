<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<FeaturesViewModel>" %>
<%@ Import Namespace="Orchard.Modules.Extensions" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Modules.ViewModels"%>
<%@ Import Namespace="Orchard.Utility.Extensions" %><%
 Html.RegisterStyle("admin.css");
 Html.RegisterStyle("jquery.switchable.css");
 Html.RegisterFootScript("jquery.switchable.js"); %>
<h1><%: Html.TitleForPage(T("Manage Features").ToString()) %></h1>
<% if (Model.Features.Count() > 0) { %>
<ul class="features summary-view switchable"><%
    var featureGroups = Model.Features.OrderBy(f => f.Descriptor.Category).GroupBy(f => f.Descriptor.Category);
    foreach (var featureGroup in featureGroups) {
        var categoryName = featureGroup.First().Descriptor.Category ?? T("Uncategorized");
        var categoryClassName = string.Format("category {0}", Html.Encode(categoryName.ToString().HtmlClassify()));
        if (featureGroup == featureGroups.First())
            categoryClassName += " first";
        if (featureGroup == featureGroups.Last())
            categoryClassName += " last";
        
        //temporarily "disable" actions on core features
        var showActions = categoryName.ToString() != "Core"; %>
    <li class="<%=categoryClassName %>">
        <h2><%: categoryName %></h2>
        <ul><%
            var features = featureGroup.OrderBy(f => f.Descriptor.Name);
            foreach (var feature in features) {
                //hmmm...I feel like I've done this before...
                var featureId = feature.Descriptor.Name.AsFeatureId(n => T(n));
                var featureState = feature.IsEnabled ? "enabled" : "disabled";
                var featureClassName = string.Format("feature {0}", featureState);
                if (feature == features.First())
                    featureClassName += " first";
                if (feature == features.Last())
                    featureClassName += " last"; %>
            <li class="<%=featureClassName %>" id="<%=Html.AttributeEncode(featureId) %>" title="<%=T("{0} is {1}", Html.AttributeEncode(feature.Descriptor.Name), featureState) %>">
                <div class="summary">
                    <div class="properties">
                        <h3><%:feature.Descriptor.Name %></h3>
                        <p class="description"><%:feature.Descriptor.Description %></p><%
                        if (feature.Descriptor.Dependencies != null) { %>
                        <div class="dependencies">
                            <h4><%: T("Depends on:")%></h4>
                            <%: Html.UnorderedList(
                                feature.Descriptor.Dependencies.OrderBy(s => s),
                                (s, i) => Html.Link(s, string.Format("#{0}", s.AsFeatureId(n => T(n)))).ToString(),
                                "",
                                "dependency",
                                "") %>
                        </div><%
                        } %>
                    </div><%
                    if (showActions) { %>
                    <div class="actions"><%
                        if (feature.IsEnabled) {
                        using (Html.BeginFormAntiForgeryPost(string.Format("{0}", Url.Action("Disable", new { area = "Orchard.Modules" })), FormMethod.Post, new {@class = "inline link"})) { %>
                            <%: Html.Hidden("id", feature.Descriptor.Name, new { id = "" })%>
                            <%: Html.Hidden("force", true)%>
                            <button type="submit"><%: T("Disable") %></button><%
                        }
                        } else {
                        using (Html.BeginFormAntiForgeryPost(string.Format("{0}", Url.Action("Enable", new { area = "Orchard.Modules" })), FormMethod.Post, new {@class = "inline link"})) { %>
                            <%: Html.Hidden("id", feature.Descriptor.Name, new { id = "" })%>
                            <%: Html.Hidden("force", true)%>
                            <button type="submit"><%: T("Enable") %></button><%
                        }
                        } %>
                    </div><%
                    } %>
                </div>
            </li><%
            } %>
        </ul>
    </li><%
    } %>
</ul><%
} %>   