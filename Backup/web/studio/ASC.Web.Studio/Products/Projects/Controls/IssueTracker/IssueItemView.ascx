<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IssueItemView.ascx.cs" 
Inherits="ASC.Web.Projects.Controls.IssueTracker.IssueItemView" %>

<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>

<div id="pmtask_<%= Target.ID %>" class="borderBase pm_issue_item <%=(RowIndex % 2 ==0)?"tintMedium":"tintLight"%>" style="border-right:none; border-left:none; border-top:none; ">
    <table class="pm-tablebase pm-issue-block" cellpadding="15" cellspacing="0">
        <tr <%=  RenderHighlightRow(Target, RowIndex) %> id="issueRow_<%= Target.ID %>">
            <td class="moveHoverBackground borderBase" style="padding:0px; background-color:Transparent; border-top:none; border-bottom:none; border-left:none; width:18px;">
                &nbsp;
            </td>
            <% if (MultiSelect) %>
            <% { %>
            <td class="pm-issue-checkbox" >
                <input type="checkbox" />
            </td>
            <% } %>
            <td class="pm-issue-status" style="white-space: nowrap;">
                <%= Status %>
            </td>
            <td class="pm-issue-id" style="white-space: nowrap;">
                <%= issueID %>
            </td>
            <td class="pm-issue-title">
                <%= RenderTitle(Target)%>
            </td>
            <td class="pm-issue-assignedOn">
                <% if (AssignedOn != new Guid("00000000-0000-0000-0000-000000000000"))
                   { %>
                    <%= StudioUserInfoExtension.RenderProfileLink(ASC.Core.CoreContext.UserManager.GetUsers(AssignedOn), ProductEntryPoint.ID)%>
                <% }
                   else
                   { %>
                       <%= IssueTrackerResource.NotAssigned%>
                <% } %>
            </td>
        </tr>
    </table>
</div>