<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>


            </div><%-- yui-b --%>
        </div><%-- yui-main --%>
        <%-- Navigation --%>
        <div class="yui-b">
            <div class="leftNavMod">
                <h4>
                    Dashboard</h4>
            </div>
            <div class="leftNavMod">
                <h4>
                    Pages</h4>
                <ul>
                    <li><%=Html.ActionLink("Manage Pages", "Index", new {area="Orchard.CmsPages",controller="Admin"}) %></li>
                    <li><%=Html.ActionLink("Add a Page", "Create", new {area="Orchard.CmsPages",controller="Admin"}) %></li>
                </ul>
            </div>
            <div class="leftNavMod">
                <h4>
                    Media</h4>
                <ul>
                    <li><%=Html.ActionLink("Manage Folders", "Index", new {area="Orchard.Media",controller="Admin"}) %></li>
                </ul>
            </div>
            <div class="leftNavMod">
                <h4>
                    Users</h4>
                <ul>
                    <li><%=Html.ActionLink("Manage Roles", "Index", new {area="Orchard.Roles",controller="Admin"}) %></li>
                </ul>
                <ul>
                    <li><%=Html.ActionLink("Add a Role", "Create", new {area="Orchard.Roles",controller="Admin"}) %></li>
                </ul>
            </div>
            
        </div>
    </div><%-- bd --%>
    <div id="ft" role="contentinfo">
    </div>
</div><%-- yui-t2 --%>
