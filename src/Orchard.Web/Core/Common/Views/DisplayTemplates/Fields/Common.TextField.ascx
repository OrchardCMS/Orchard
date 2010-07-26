<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Common.Fields.TextField>" %>
<%@ Import Namespace="Orchard.Utility.Extensions" %>
<p class="text-field"><span class="name"><%:Model.Name.CamelFriendly() %>:</span> <%:Model.Value %></p>