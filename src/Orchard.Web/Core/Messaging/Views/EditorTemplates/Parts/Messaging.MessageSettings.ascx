<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentSubscriptionPartViewModel>" %>
<%@ Import Namespace="Orchard.Core.Messaging.Models"%>
<%@ Import Namespace="Orchard.Core.Messaging.ViewModels"%>
<fieldset>
    <legend><%: T("Messaging")%></legend>
    <div>
        <label for="<%: Html.FieldIdFor(m => m.MessageSettings.DefaultChannelService)%>"><%: T("Default channel service for messages")%></label>
        <% if ( Model.ChannelServices.Any() ) { %>
            <select id="<%:Html.FieldIdFor(m => m.MessageSettings.DefaultChannelService) %>" name="<%:Html.FieldNameFor(m => m.MessageSettings.DefaultChannelService) %>">
            <% foreach ( var service in Model.ChannelServices ) {%>
                <option <%: Model.MessageSettings.DefaultChannelService == service ? "selected=\"selected\"" : "" %> value="<%: service %>"><%: service%></option>
            <% }
           }
           else {%>
           <span class="hint"><%: T("You must enable a messaging channel (e.g., Orchard.Email) before being able to send messages.") %></span>
        <% }%>

    </select> 
    </div>
</fieldset>