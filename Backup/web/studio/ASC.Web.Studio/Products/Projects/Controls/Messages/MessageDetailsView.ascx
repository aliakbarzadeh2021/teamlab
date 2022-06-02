<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MessageDetailsView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Messages.MessageDetailsView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
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
                <%= ASC.Core.Users.UserInfoExtension.DisplayUserName(ASC.Core.CoreContext.UserManager.GetUsers(Message.CreateBy),true)%></a>
            <span class="textBigDescribe" style="padding-left:5px;">
                <%=GetMessageDate()%>
            </span>
        </div>
        <div class="textBigDescribe">
            <%= ASC.Core.CoreContext.UserManager.GetUsers(Message.CreateBy).Title.HtmlEncode()%>
        </div>
        <div class="pm-message-text-container"><%=GetMessageText()%></div>
        
        <%=GetAttachedImages()%>
        
        <div class="pm-h-line"><!– –></div>
        <%=GetActions()%>
        <% if (GetCountSubscribedPeople() != 0)%>
        <% {%>
        <div>
            <span class="textBigDescribe message-details">
                <%=ProjectsCommonResource.PepleSubscribedToMessage%>:</span>
            <br />
            <br />
            <div class="clearFix" style="overflow:hidden;">
                <%=GetSubscribedPeople()%></div>
        </div>
        <% }%>
    </div>
</div>
<ascw:CommentsList ID="commentList" runat="server" BehaviorID="commentsList">
</ascw:CommentsList>