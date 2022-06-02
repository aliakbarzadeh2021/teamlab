<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ClosedTaskBlockViewRow.ascx.cs" Inherits="ASC.Web.Projects.Controls.Tasks.ClosedTaskBlockViewRow" %>

<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>


<div id="pmtask_<%= Target.ID %>_<%= Target.Milestone%>" class="borderBase pm-task-row">
    <% if (!IsAllMyTasks) %>
    <% { %>
    <table class="pm-tablebase pm-tasks-block" cellpadding="10" cellspacing="0">
    <% } %>
    <% else %>
    <% { %>
    <table class="pm-tablebase pm-tasks-block" cellpadding="5" cellspacing="0">
    <% } %>
            <tr id="taskRow_<%= Target.ID %>">
                <td class="pm-task-checkbox">
                    <% if (HavePermission && Target.Responsible != Guid.Empty && Target.Status != TaskStatus.Unclassified && IsOpenMilestone) %>
                    <% { %>
                    <input type="checkbox" id="taskStatus_<%= Target.ID %>" checked="checked" onclick="javascript:ASC.Projects.TaskActionPage.changeStatus(<%=Target.ID%>,<%= Target.Milestone%>,<%=Target.Project.ID %>)" title="<%= TaskResource.ClosedTaskCheckBoxTooltip %>" />
                    <img id="loaderImg_<%= Target.ID %>" src="<%=GetLoaderImgURL() %>" style="display:none;margin-left:5px"/>
                    <% } %>
                    <% else %>
                    <% { %>
                    <input type="checkbox" id="taskStatus_<%= Target.ID %>" disabled="disabled" checked="checked" />
                    <% } %>
                </td>
                <td class="pm-task-title">
                    <div <%=GetTitleContainerWidth()%>>
                        <%= GetTitle()%>  
                    </div>                 
                </td>
                <% if (Global.ModuleManager.IsVisible(ModuleType.TimeTracking)) %>
                <% { %>
                <td class="pm-timetracking">
                </td>
<% if (OneList) %>
        <% { %>
            <td class="pm_taskBlockDescribe" style="float:right; text-align:right; padding-top:9px;">
                <div style="width:400px;overflow-x: hidden;">
                 <%=string.IsNullOrEmpty(ProjectName) ? "" : "<span class='projectIcon_grey'>"+ProjectName+"</span>" %> 
                 <%=string.IsNullOrEmpty(MilestoneName) ? "" : "<span class='milestoneIcon_grey'>"+MilestoneName+"</span>" %> 
                </div>
            </td>
        <% } %>
                <% } %>
                <% if (!IsAllMyTasks) %>
                <% { %>
                <td class="pm-timetracking">
                    <% if (Target.Responsible != Guid.Empty) %>
                    <% { %>
                    <img src="<%=WebImageSupplier.GetAbsoluteWebPath("profile.gif")%>" id="imgProfile_<%=Target.ID%>" />
                    <% } %>
                </td>
                <td class="pm-task-responsible">
                    <div style="color: #9CA0A4 !important;font-family: Arial;font-size: 11px; overflow: hidden; width: 130px;"><%= GetUserName()%></div>
                </td>
                <% } %>
            </tr>
    </table>
</div>
