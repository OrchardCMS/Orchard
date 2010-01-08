<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<ContentDetailsViewModel>" %>
<%@ Import Namespace="Orchard.DevTools.ViewModels"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="System.Reflection" %>
<h1><%=Html.TitleForPage(T("{0} Content Type", Model.Item.ContentItem.ContentType).ToString(), T("Content").ToString())%></h1>
<h2><%=_Encoded("Content Item")%></h2>
<p>
<%=_Encoded("Id:")%>
    <%=Model.Item.ContentItem.Id %><br />
<%=_Encoded("Version:")%>
    <%=Model.Item.ContentItem.Version %><br />
<%=_Encoded("ContentType:")%>
    <%=Model.Item.ContentItem.ContentType %><br />
<%=_Encoded("DisplayText:")%> 
    <%=Html.ItemDisplayText(Model.Item) %><br />
<%=_Encoded("Links:")%> 
    <%=Html.ItemDisplayLink(T("view").ToString(), Model.Item) %> <%=Html.ItemEditLink(T("edit").ToString(), Model.Item) %>
</p>
<h2><%=_Encoded("Content Item Parts")%></h2>
<ul>
    <%foreach (var partType in Model.PartTypes.OrderBy(x => x.Name)) {%>
    <li><span style="font-weight: bold;">
        <%if (partType.IsGenericType) {%><%=Html.Encode(partType.Name +" "+partType.GetGenericArguments().First().Name)%></span>
        <%=Html.Encode(" (" + partType.GetGenericArguments().First().Namespace + ")")%><%}
          else {%><%=Html.Encode(partType.Name)%></span>
        <%=Html.Encode( " (" + partType.Namespace + ")")%><%
                                                            }
          
          %>
        <ul style="margin-left: 20px">
            <%foreach (var prop in partType.GetProperties().Where(x => x.DeclaringType == partType)) {
                  var value = prop.GetValue(Model.Locate(partType), null);%>
            <li style="font-weight: normal;">
                <%=Html.Encode(prop.Name) %>:
                <%=Html.Encode(value) %>
                <%var valueItem = value as ContentItem;
                  if (valueItem == null && value is IContent) {
                      valueItem = (value as IContent).ContentItem;
                  }
                  if (valueItem != null) {
                      %><%=Html.ActionLink(T("{0} #{1} v{2}", valueItem.ContentType, valueItem.Id).ToString(), "details", new { valueItem.Id }, new { })%><%
                  }
                  %>
                <ul style="margin-left: 20px">
                    <%if (value == null || prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string)) { }
                      else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)) {
                          foreach (var item in value as IEnumerable) {
                    %>
                    <li><%=Html.Encode(item.GetType().Name) %>:<%=Html.Encode(item) %></li>
                    <%
                        }

                      }
                      else {%>
                    <%foreach (var prop2 in value.GetType().GetProperties().Where(x => x.GetIndexParameters().Count() == 0)) {%>
                    <li>
                        <%=Html.Encode(prop2.Name)%>
                        <%=Html.Encode(prop2.GetValue(value, null))%></li>
                    <%} %>
                    <%} %>
                </ul>
            </li>
            <%} %>
        </ul>
    </li>
    <%}%>
</ul>


<h3>Displays</h3>
<ul>
    <%foreach (var display in Model.Displays) {%>
    <li><span style="font-weight: bold">
        <%=Html.Encode(display.Prefix)%></span>
        <%=Html.Encode(display.Model.GetType().Name)%>
        (<%=Html.Encode(display.Model.GetType().Namespace)%>)
        Template:<%=Html.Encode(display.TemplateName ?? "(null)")%>
        Prefix:<%=Html.Encode(display.Prefix ?? "(null)")%>
        Zone:<%=Html.Encode(display.ZoneName ?? "(null)")%>
        Position:<%=Html.Encode(display.Position ?? "(null)")%>
        <div style="margin-left: 20px; border: solid 1px black;">
            <%=Html.DisplayFor(x => display.Model, display.TemplateName, display.Prefix)%>
        </div>
    </li>
    <%                      
        }%>
</ul>


<h3>Editors</h3>
<ul>
    <%foreach (var editor in Model.Editors) {%>
    <li><span style="font-weight: bold">
        <%=Html.Encode(editor.Prefix) %></span>
        <%=Html.Encode(editor.Model.GetType().Name) %>                    
        (<%=Html.Encode(editor.Model.GetType().Namespace) %>)
        Template:<%=Html.Encode(editor.TemplateName ?? "(null)")%>
        Prefix:<%=Html.Encode(editor.Prefix ?? "(null)")%>
        Zone:<%=Html.Encode(editor.ZoneName ?? "(null)")%>
        Position:<%=Html.Encode(editor.Position??"(null)") %>
        <div style="margin-left: 20px; border: solid 1px black;">
            <%=Html.EditorFor(x=>editor.Model, editor.TemplateName, editor.Prefix) %>
        </div>
    </li>
    <%                      
        }%>
</ul>
