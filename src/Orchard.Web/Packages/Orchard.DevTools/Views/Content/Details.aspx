<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.DevTools.ViewModels.ContentDetailsViewModel>" %>
<%@ Import Namespace="Orchard.Models"%>

<%@ Import Namespace="System.Reflection" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>

<h3>Content Item</h3>
<p>
    Id:
    <%=Model.Item.ContentItem.Id %></p>
<p>
    ContentType:
    <%=Model.Item.ContentItem.ContentType%> <%=Html.ItemDisplayLink(Model.Item) %> <%=Html.ItemEditLink("edit", Model.Item) %></p>


<h3>Content Item Parts</h3>
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
                      %><%=Html.ActionLink(valueItem.ContentType + " #" + valueItem.Id, "details", new { valueItem.Id }, new { })%><%
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
