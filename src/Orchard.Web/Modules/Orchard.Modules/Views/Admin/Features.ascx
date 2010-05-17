<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<FeaturesViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Modules.ViewModels"%>
<%@ Import Namespace="Orchard.Utility.Extensions" %><%
 Html.RegisterStyle("admin.css"); %>
<h1><%=Html.TitleForPage(T("Manage Features").ToString()) %></h1>
<% if (Model.Features.Count() > 0) { %>
<ul class="features"><%
    var featureGroups = Model.Features.OrderBy(f => f.Descriptor.Category).GroupBy(f => f.Descriptor.Category);
    foreach (var featureGroup in featureGroups) {
        var categoryName = featureGroup.First().Descriptor.Category ?? T("Uncategorized");
        var categoryClassName = string.Format("category {0}", Html.Encode(categoryName.ToString().HtmlClassify()));
        if (featureGroup == featureGroups.First())
            categoryClassName += " first";
        if (featureGroup == featureGroups.Last())
            categoryClassName += " last"; %>
    <li class="<%=categoryClassName %>">
        <h2><%=Html.Encode(categoryName) %></h2>
        <ul><%
            var features = featureGroup.OrderBy(f => f.Descriptor.Name);
            foreach (var feature in features) {
                //hmmm...I feel like I've done this before...
                var featureId = string.Format("{0} feature", feature.Descriptor.Name).HtmlClassify();
                var featureClassName = string.Format("feature {0}", feature.IsEnabled ? "enabled" : "disabled");
                if (feature == features.First())
                    featureClassName += " first";
                if (feature == features.Last())
                    featureClassName += " last"; %>
            <li class="<%=featureClassName %>" id="<%=Html.AttributeEncode(featureId) %>">
                <div class="summary">
                    <div class="properties">
                        <h3><%=Html.Encode(feature.Descriptor.Name) %></h3><%
                        if (feature.Descriptor.Dependencies != null) { %>
                        <div class="dependencies">
                            <h4><%=_Encoded("Depends on:")%></h4>
                            <%=Html.UnorderedList(
                                feature.Descriptor.Dependencies.OrderBy(s => s),
                                (s, i) => Html.Link(s, string.Format("#{0}", string.Format("{0} feature", s).HtmlClassify())),
                                "",
                                "dependency",
                                "") %>
                        </div><%
                        } %>
                    </div>
                    <div class="actions"><%
                        if (feature.IsEnabled) {
                        using (Html.BeginFormAntiForgeryPost(string.Format("{0}#{1}", Url.Action("Disable", new { area = "Orchard.Modules" }), featureId), FormMethod.Post, new {@class = "inline link"})) { %>
                            <%=Html.Hidden("id", feature.Descriptor.Name, new { id = "" })%>
                            <button type="submit"><%=_Encoded("Disable") %></button><%
                        }
                        } else {
                        using (Html.BeginFormAntiForgeryPost(string.Format("{0}#{1}", Url.Action("Enable", new { area = "Orchard.Modules" }), featureId), FormMethod.Post, new {@class = "inline link"})) { %>
                            <%=Html.Hidden("id", feature.Descriptor.Name, new { id = "" })%>
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