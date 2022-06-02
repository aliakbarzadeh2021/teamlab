<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>


<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskBlockViewRow.ascx.cs" Inherits="ASC.Web.Projects.Controls.Tasks.TaskBlockViewRow" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>

<div 
     <% if (!IsAllMyTasks) %>
     <% { %>
     onmouseover="javascript:ASC.Projects.TaskActionPage.taskItem_mouseOver(this);"
     onmouseout="javascript:ASC.Projects.TaskActionPage.taskItem_mouseOut(this);"
     <% } %> 
     id="pmtask_<%= Target.ID %>_<%= Target.Milestone%>"
     class="<%=GetRowCssClass()%>"
     style="border-right:none; border-left:none; border-top:none;">
     
    <% if (!IsAllMyTasks) %>
    <% { %>
    <table class="pm-tablebase pm-tasks-block" cellpadding="10" cellspacing="0">   
    <% } %>
    <% else %>
    <% { %>
    <table class="pm-tablebase pm-tasks-block" cellpadding="5" cellspacing="0">
    <% } %>
    
        <tr <%=  RenderHighlightRow(Target, RowIndex) %> id="taskRow_<%= Target.ID %>">
        
        <td class="pm-task-checkbox" >
            <% if (HaveFullPermission) %>
            <% { %>
            <div style="position:relative">
                <div id="drag_<%= Target.ID %>" class="borderBase moveHoverBackground pm-moveHoverBackground pm-tasksItem-drag" style="background-image:url('<%=WebImageSupplier.GetAbsoluteWebPath("drag.png", ProductEntryPoint.ID)%>');">
                    &nbsp;
                </div>
            </div>
            <% } %>
            <% if (HavePermission && Target.Responsible != Guid.Empty && Target.Status != TaskStatus.Unclassified) %>
            <% { %>    
            <input type="checkbox" id="taskStatus_<%= Target.ID %>" <%= Target.Status == TaskStatus.Closed ? "checked='checked'" : "" %> onchange="javascript:ASC.Projects.TaskActionPage.changeStatus(<%=Target.ID%>)" />
            <% } %>
            <% else %>
            <% { %>
            <input type="checkbox" id="taskStatus_<%= Target.ID %>" disabled="disabled" <%= Target.Status == TaskStatus.Closed ? "checked='checked'" : "" %> />
            <% } %>
        </td>

        <td class="pm-task-title">
            <%= GetTitle()%>
            <span class="splitter"></span>
            <%= GetTaskDeadline()%>
            <% if (OneList) %>
            <% { %>
            <div class="pm_taskBlockDescribe">
                 <%=string.IsNullOrEmpty(ProjectName) ? "" : "<span class='projectIcon_grey'></span>" + ProjectName%> 
                 <%=string.IsNullOrEmpty(MilestoneName) ? "" : "<span class='milestoneIcon_grey'></span>" + MilestoneName%> 
            </div>
            <% } %>
        </td>
        <% if (Global.ModuleManager.IsVisible(ModuleType.TimeTracking)) %>
        <% { %>
        <td class="pm-timetracking" style="padding-top:9px;">
            <% if (Target.Responsible != Guid.Empty) %>
            <% { %>
            <img style="display:none;border:none;" onclick='<%= GetTimeTrackingAction()%>'  src="<%= GetTimeTrackingImagePath()  %>" title="<%= CommonResource.TimeTracking %>" alt="<%= CommonResource.TimeTracking %>" id="imgTime_<%=Target.ID%>" />
            <% } %>
        </td>
        <% } %>
        <% if (!IsAllMyTasks) %>
        <% { %>
        <td class="pm-timetracking">
            <% if (Target.Responsible != Guid.Empty) %>
            <% { %>
            <img src="<%=WebImageSupplier.GetAbsoluteWebPath("profile.gif")%>" style="display:block;" id="imgProfile_<%=Target.ID%>" />
            <img src="<%=WebImageSupplier.GetAbsoluteWebPath("bell.png",ProductEntryPoint.ID)%>" style="display:none;" onclick='<%= GetNotifyAction()%>' id="imgBell_<%=Target.ID%>" title="<%= CommonResource.NotifyPeopleViaEmail %>" />
            <% } %>
        </td>
        <td class="pm-task-responsible">
            <span style="color: #646567;"><%= GetUserName()%></span>
            
            <% if (HavePermission) %>
            <% { %>
            <div id="actionBlock_<%= Target.ID %>" style="position:absolute;display:none;width:40px;">
                <div id="actions_<%= Target.ID %>" class="borderBase pm-tasksItem-actions" style="float:right;">
                    <img style="cursor:pointer;"src="<%=WebImageSupplier.GetAbsoluteWebPath("edit_small.png", ProductEntryPoint.ID)%>" title="<%=CommonResource.Edit %>" onclick="javascript:ASC.Projects.TaskActionPage.init(<%=Target.ID%>, null, null);ASC.Projects.TaskActionPage.show();"/><br />
                    <img style="padding:2px 2px 2px 3px;cursor:pointer;" src="<%=WebImageSupplier.GetAbsoluteWebPath("arr_right.png", ProductEntryPoint.ID)%>" title="<%=TaskResource.MoveToMilestone %>" onclick="javascript:ASC.Projects.TaskActionPage.showMoveTaskDialog(<%=Target.ID%>, <%= Target.Milestone%>);" /><br />
                    <img style="padding:2px 2px 2px 3px;cursor:pointer;" src="<%=WebImageSupplier.GetAbsoluteWebPath("trash_fu.png", ProductEntryPoint.ID)%>" title="<%=CommonResource.Delete %>" onclick="javascript:ASC.Projects.TaskActionPage.deleteTaskItem(<%=Target.ID%>, <%= Target.Milestone%>);"/>
                </div>
                <div id="actionsPointer_<%= Target.ID %>" class="borderBase pm-tasksItem-actionsPointer" style="background-image:url('<%=WebImageSupplier.GetAbsoluteWebPath("min_collapce.gif", ProductEntryPoint.ID)%>');">
                    &nbsp;
                </div>
                <img id="imgZub_<%= Target.ID %>" src="<%=WebImageSupplier.GetAbsoluteWebPath("zub_task.png", ProductEntryPoint.ID)%>" style="position:relative;left: 14px;"/>
            </div>
            <% } %>
        </td>
        <% } %>
        
        </tr>
    </table>
    
</div>
