
<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentTypesIndexViewModel>" %>
<%@ Import Namespace="Orchard.MetaData.ViewModels"%>
<% Html.RegisterStyle("ContentTypes.css"); %>

<div class="ContentTypeList">
    <table>
        <tr>

            <th>
                Content Types
            </th>
        </tr>

    <% foreach (var item in Model.ContentTypes) { %>
    
    <%
           var contentTypeClass = "";
           if (Model.SelectedContentType!=null && Model.SelectedContentType.Name == item.Name)
           {
                contentTypeClass = "SelectedContentPart";
            }else{
                contentTypeClass = "UnSelectedContentPart";
            }       
    %>
        <tr class="<%=contentTypeClass %>">
            <td>
                <%: Html.ActionLink(item.Name, "ContentTypeList", new {id=item.Name})%>
            </td>
        </tr>
    
    <% } %>

    </table>
</div>

<%if (Model.SelectedContentType!=null) {%>

<div class="ContentTypePartList">
    <table>
        <tr>
            <th>
                
            </th>
            <th>
                Included Content Part
            </th>
        </tr>
<%
using (Html.BeginFormAntiForgeryPost(Url.Action("Save",new {id=Model.SelectedContentType.Name}))) { %>

    <% foreach (var item in Model.ContentTypeParts) { %>
    
        <tr class="ContentTypePartListRow">
            <td class="ContentTypePartListRowItem">
            <%if (item.Selected)
              {%>
            <input name="<%="part_" + item.Name%>" type="checkbox" checked="checked" /><%}
              else {%>
            <input name="<%="part_" + item.Name%>" type="checkbox" /><%}%>
            </td>
            <td class="ContentTypePartListRowItem">
                <%: item.Name%>
            </td>
        </tr>
    
    <% } %>

    </table>
    <p>
        <input type="submit" value="<%: T("Save") %>" />
    </p>
    <% } %>
</div>
<%} %>

    


