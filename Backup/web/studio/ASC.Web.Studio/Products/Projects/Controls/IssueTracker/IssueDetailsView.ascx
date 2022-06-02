<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Projects.Engine" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IssueDetailsView.ascx.cs" 
Inherits="ASC.Web.Projects.Controls.IssueTracker.IssueDetailsView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Projects.Engine" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Core.Users" %>

<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>
<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>

<div id="mainpanel">
    <dl class="pm-flexible">
    
        <dt class="textBigDescribe">
            <%= IssueTrackerResource.IssueID %>
        </dt>
        <dd>
            <%= Target.ID %>
            
            <span class="splitter"></span>
            <span class="textBigDescribe"><%= IssueTrackerResource.DetectedInVersion%>:</span>
            <span style="padding-left:5px;"><%= Target.DetectedInVersion %></span>
        </dd>
    
        <dt class="textBigDescribe">
            <%= IssueTrackerResource.CreateOn %>: 
        </dt>
        <dd>
            <%= Target.CreateOn.ToString(System.DateTimeExtension.DateFormatPattern)%>
            
            <% if (!String.IsNullOrEmpty(Target.CorrectedInVersion.Trim())) %>
            <% { %>
                <span class="splitter"></span>
                <span class="textBigDescribe"><%= IssueTrackerResource.CorrectedInVersion%>:</span>
                <span style="padding-left:5px;"><%= Target.CorrectedInVersion%></span>
            <% } %>
        </dd>
        
        <dt class="textBigDescribe">
            <%= IssueTrackerResource.CreateBy %>
        </dt>
        <dd>        
            <%= StudioUserInfoExtension.RenderProfileLink(ASC.Core.CoreContext.UserManager.GetUsers(Target.CreateBy), ProductEntryPoint.ID)%>            
            <span class="splitter"></span>
            <span class="textBigDescribe"><%= IssueTrackerResource.AssignedOn%>:</span>
            <span style="padding-left:5px;">
            <% if (Target.AssignedOn != new Guid("00000000-0000-0000-0000-000000000000"))
                { %>
                    <%= StudioUserInfoExtension.RenderProfileLink(ASC.Core.CoreContext.UserManager.GetUsers(Target.CreateBy), ProductEntryPoint.ID)%>
             <% }
                else
                { %>
                    <%= IssueTrackerResource.NotAssigned%>
             <% } %></span>
        </dd>
        
        <% if (!String.IsNullOrEmpty(Target.Description.Trim())) %>
        <% { %>
        <dt class="textBigDescribe">
            <%= IssueTrackerResource.Description%>: 
        </dt>
        <dd>
        <%= Target.Description.Replace("\n","<br/>").Trim()%>
        </dd>
        <% } %>
        
        <dt class="textBigDescribe" style="clear:left">
            <div><%= ProjectsCommonResource.Status%>:</div> 
        </dt>
        <dd>
        
            <%= RenderStatus(Target.Status)%>
            
            <span class="splitter"></span>
            <span class="textBigDescribe"><%= IssueTrackerResource.Priority %>:</span>
            <span style="padding-left:5px;"><%= RenderPriorityBlock(Target.Priority) %></span>
            
        </dd>
    </dl>
</div>

<ascw:CommentsList ID="commentList" runat="server" BehaviorID="commentsList">
</ascw:CommentsList>