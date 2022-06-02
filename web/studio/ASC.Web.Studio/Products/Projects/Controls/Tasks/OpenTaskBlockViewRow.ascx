<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>


<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OpenTaskBlockViewRow.ascx.cs" Inherits="ASC.Web.Projects.Controls.Tasks.OpenTaskBlockViewRow" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>


<div 
     <% { %>
     onmouseover="javascript:ASC.Projects.TaskActionPage.taskItem_mouseOver(this);"
     onmouseout="javascript:ASC.Projects.TaskActionPage.taskItem_mouseOut(this);"
     <% } %> 
     id="pmtask_<%= Target.ID %>_<%= Target.Milestone%>"
     class="<%=GetRowCssClass()%>">
     
    <% if (!IsAllMyTasks) %>
    <% { %>
    <table class="pm-tablebase pm-tasks-block" cellpadding="10" cellspacing="0">   
    <% } %>
    <% else %>
    <% { %>
    <table class="pm-tablebase pm-tasks-block" cellpadding="5" cellspacing="0">
    <% } %>
    
        <tr id="taskRow_<%= Target.ID %>">
        
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
            <input type="checkbox" id="taskStatus_<%= Target.ID %>" onclick="javascript:ASC.Projects.TaskActionPage.changeStatus(<%=Target.ID%>,<%= Target.Milestone%>,<%= Target.Project.ID%>)" title="<%= TaskResource.ActiveTaskCheckBoxTooltip %>" />
            <img id="loaderImg_<%= Target.ID %>" src="<%=GetLoaderImgURL() %>" style="display:none;margin-left:5px"/>
            <% } %>
            <% else %>
            <% { %>
            <input type="checkbox" id="taskStatus_<%= Target.ID %>" disabled="disabled" />
            <% } %>
        </td>

        <td class="pm-task-title">
        <div <%=GetTitleContainerWidth()%>>
            <%= GetTitle()%>
            <%= GetTaskDeadline()%>
            <% if (IsAllMyTasks) %>
            <% { %>
            <span class="splitter"></span>
            <img style="margin: -2px 0;<%=HasTime?"":"display:none;"%>border:none;cursor:pointer;" hastime='<%= HasTime ? 1 : 0%>' onclick='<%= GetTimeTrackingAction()%>' src="<%= GetTimeTrackingImagePath()  %>" title="<%= ProjectsCommonResource.TimeTracking %>" alt="<%= ProjectsCommonResource.TimeTracking %>" id="imgTime_<%=Target.ID%>" />
            <% } %>
        </div>
        </td>
        <% if (Global.ModuleManager.IsVisible(ModuleType.TimeTracking) && !OneList) %>
        <% { %>
        <td class="pm-timetracking" style="padding-top:9px;">
            <% if (Target.Responsible != Guid.Empty && !IsAllMyTasks) %>
            <% { %>
            <img style="<%=HasTime?"":"display:none;"%>border:none;cursor:pointer;" hastime='<%= HasTime ? 1 : 0%>' onclick='<%= GetTimeTrackingAction()%>' src="<%= GetTimeTrackingImagePath()  %>" title="<%= ProjectsCommonResource.TimeTracking %>" alt="<%= ProjectsCommonResource.TimeTracking %>" id="imgTime_<%=Target.ID%>" />
            <% } %>
        </td>
        <% } %>
        <% if (!IsAllMyTasks) %>
        <% { %>
        <td class="pm-timetracking">
            <% if (Target.Responsible != Guid.Empty) %>
            <% { %>
            <img src="<%=WebImageSupplier.GetAbsoluteWebPath("profile.gif")%>" style="display:block;" id="imgProfile_<%=Target.ID%>" />
            <% if (Target.Responsible != ASC.Core.SecurityContext.CurrentAccount.ID) %>
            <% { %>
            <img src="<%=WebImageSupplier.GetAbsoluteWebPath("bell.png",ProductEntryPoint.ID)%>" style="display:none;cursor:pointer;" onclick='<%= GetNotifyAction()%>' id="imgBell_<%=Target.ID%>" title="<%= TaskResource.Remind %>" />
            <% } %>
            <% } %>
        </td>
        <% } %>
        <% if (OneList) %>
        <% { %>
            <td class="pm_taskBlockDescribe" style="float:right; text-align:right; padding-top:9px;">
                <div style="width:400px;overflow-x: hidden;">
                 <%=string.IsNullOrEmpty(ProjectName) ? "" : "<span class='projectIcon_grey'>"+ProjectName.HtmlEncode()+"</span>" %> 
                 <%=string.IsNullOrEmpty(MilestoneName) ? "" : "<span class='milestoneIcon_grey'>"+MilestoneName.HtmlEncode()+"</span>" %> 
                </div>
            </td>
        <% } %>
        
        <% if (!IsAllMyTasks) %>
        <% { %>
        <td class="pm-task-responsible">
            <div style="color: #646567; overflow: hidden; width: 130px; line-height:20px;"><%= GetUserName()%></div>
            
            <% if (HavePermission) %>
            <% { %>
            <div id="actionBlock_<%= Target.ID %>" style="position:absolute;display:none;width:40px;">
                <div id="actions_<%= Target.ID %>" class="borderBase pm-tasksItem-actions" style="float:right;">
                    <img style="cursor:pointer;"src="<%=WebImageSupplier.GetAbsoluteWebPath("edit_small.png", ProductEntryPoint.ID)%>" title="<%=ProjectsCommonResource.Edit %>" onclick="javascript:ASC.Projects.TaskActionPage.init(<%=Target.ID%>, null, null);ASC.Projects.TaskActionPage.show(<%=Target.ID%>);"/>
                    <% if (ASC.Core.SecurityContext.CurrentAccount.ID == Target.Responsible) %>
                    <% { %>
                    <br />
                    <img style="padding-left:1px;cursor:pointer;" src="<%=WebImageSupplier.GetAbsoluteWebPath("start-track.png", ProductEntryPoint.ID)%>" title="<%=ProjectsCommonResource.AutoTimer %>" onclick="javascript:ASC.Projects.TimeSpendActionPage.showTimer('timetracking.aspx?prjID=<%=ProjectFat.Project.ID%>&ID=<%=Target.ID%>&action=timer');" />
                    <% } %>
                    <br />
                    <img style="padding:2px 2px 2px 3px;cursor:pointer;" src="<%=WebImageSupplier.GetAbsoluteWebPath("arr_right.png", ProductEntryPoint.ID)%>" title="<%=TaskResource.MoveToMilestone %>" onclick="javascript:ASC.Projects.TaskActionPage.showMoveTaskDialog(<%=Target.ID%>, <%= Target.Milestone%>);" />
                    <br />
                    <img style="padding:2px 2px 2px 3px;cursor:pointer;" src="<%=WebImageSupplier.GetAbsoluteWebPath("trash_fu.png", ProductEntryPoint.ID)%>" title="<%=ProjectsCommonResource.Delete %>" onclick="javascript:ASC.Projects.TaskActionPage.deleteTaskItem(<%=Target.ID%>, <%= Target.Milestone%>);"/>
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
