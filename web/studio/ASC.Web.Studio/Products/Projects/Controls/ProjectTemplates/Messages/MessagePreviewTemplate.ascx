<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MessagePreviewTemplate.ascx.cs" Inherits="ASC.Web.Projects.Controls.ProjectTemplates.Messages.MessagePreviewTemplate" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Projects.Engine" %>


<div class="message-container" id="message_<%=GetMessageID()%>">

<div style="padding:10px;" class="clearFix">
    
    <% if (IsPreview)%>
    <% {%>
    <div style="padding-bottom:20px;">
        <table cellspacing="0" cellpadding="0">    
            <tbody>
                <tr>
                    <td style="padding-right: 15px;">
                        <%= ASC.Web.Projects.Classes.Global.RenderEntityPlate(EntityType.Message, true)%>
                    </td>
                    <td>
                        <span style="color:#111111;font-family:Arial;font-size:24px;"><%=GetMessageTitle()%></span>
                    </td>
                </tr>
            </tbody>
        </table> 
    </div>
    <% } %>
    
    <div class="pm-message-left-side">
        <img src="<%=GetAvatarURL()%>" />
    </div>
    
    <div class="pm-message-right-side">
    
        <% if (!IsPreview)%>
        <% {%>
            <div style="padding-bottom:3px;">
            <a style="font-weight:bolder" class="linkHeaderLight" href="<%=GetHrefMessage() %>">
                <%=GetMessageTitle()%>
            </a>
        </div>
        <% } %>  
        
        <div>
            <a class="linkHeader" href="<%= GetHrefUser() %>">
                    <%= GetUserName()%>
            </a>
            <span class="textBigDescribe" style="padding: 0px 0px 5px;">
                <%=GetMessageDate()%>
            </span>
        </div>
        
        <div class="textBigDescribe">
                <%= GetUserTitle()%>
        </div>
        
        <div class="pm-message-text-container">
            <%=GetMessageText()%>
        </div>
    </div>
</div>
    
    
    
</div>


