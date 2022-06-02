<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MessageTemplateDetails.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.ProjectTemplates.Messages.MessageTemplateDetails" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Engine" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>
<div class="clearFix">
    <div class="pm-message-left-side">
        <img src="<%=GetAvatarURL()%>" />
    </div>
    <div class="pm-message-right-side">
        <div>
            <a class="linkHeader" href="<%= ASC.Core.Users.StudioUserInfoExtension.GetUserProfilePageURL(ASC.Core.CoreContext.UserManager.GetUsers(Message.CreateBy), ProductEntryPoint.ID) %>">
                <%= ASC.Core.Users.UserInfoExtension.DisplayUserName(ASC.Core.CoreContext.UserManager.GetUsers(Message.CreateBy))%></a>
            <span class="textBigDescribe" style="padding-left: 5px;">
                <%=GetMessageDate()%>
            </span>
        </div>
        <div class="textBigDescribe">
            <%= ASC.Core.CoreContext.UserManager.GetUsers(Message.CreateBy).Title%>
        </div>
        <div class="pm-message-text-container">
            <%=GetMessageText()%></div>
    </div>
</div>