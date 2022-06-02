<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>


<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskBlockViewTemplateRow.ascx.cs" Inherits="ASC.Web.Projects.Controls.ProjectTemplates.Tasks.TaskBlockViewTemplateRow" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>


<div onmouseover="ASC.Projects.TaskActionPage.taskItem_mouseOver(this);"
     onmouseout="ASC.Projects.TaskActionPage.taskItem_mouseOut(this);"
     id="pmtask_<%= Target.Id %>_<%= Target.MilestoneId%>"
     class="<%=GetRowCssClass()%>">
     
    <table class="pm-tablebase pm-tasks-block" cellpadding="10" cellspacing="0">   
        <tr class="pm-task-open" id="taskRow_<%= Target.Id %>">
        
        <td class="pm-task-checkbox" >
			<%if(ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesUtil.CheckEditPermission(Template)) { %>
            <div style="position:relative">
                <div id="drag_<%= Target.Id %>" class="borderBase moveHoverBackground pm-moveHoverBackground pm-tasksItem-drag" style="background-image:url('<%=WebImageSupplier.GetAbsoluteWebPath("drag.png", ProductEntryPoint.ID)%>');">
                    &nbsp;
                </div>
            </div>
            <%} %>
            <input type="checkbox" disabled="disabled" id="taskStatus_<%= Target.Id %>" />
        </td>

        <td class="pm-task-title">
            <div style="width: 490px;">
                <%= GetTitle()%>
            </div>
        </td>
        
        <td class="projectTemplatesTaskResponsible">

            <div style="color: #646567; overflow: hidden; width: 150px; line-height: 20px;"><%= GetUserName()%></div>
            
            <%if(ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesUtil.CheckEditPermission(Template)) { %>
            <div id="actionBlock_<%= Target.Id %>" style="position:absolute;display:none;width:40px;">
                <div id="actions_<%= Target.Id %>" class="borderBase pm-tasksItem-actions" style="float:right;">
                    <img style="cursor:pointer;"src="<%=WebImageSupplier.GetAbsoluteWebPath("edit_small.png", ProductEntryPoint.ID)%>" title="<%=ProjectsCommonResource.Edit %>" onclick="javascript:ASC.Projects.TaskActionPage.init(<%=Target.Id%>, null, null);ASC.Projects.TaskActionPage.show(<%=Target.Id%>);"/><br />
                    <img style="padding:2px 2px 2px 3px;cursor:pointer;" src="<%=WebImageSupplier.GetAbsoluteWebPath("arr_right.png", ProductEntryPoint.ID)%>" onclick="javascript:ASC.Projects.TaskActionPage.showMoveTaskDialog(<%=Target.Id%>, <%= Target.MilestoneId%>);" /><br />
                    <img style="padding:2px 2px 2px 3px;cursor:pointer;" src="<%=WebImageSupplier.GetAbsoluteWebPath("trash_fu.png", ProductEntryPoint.ID)%>" title="<%=ProjectsCommonResource.Delete %>" onclick="javascript:ASC.Projects.TaskActionPage.deleteTaskItem(<%=Target.Id%>, <%= Target.MilestoneId%>);"/>
                </div>
                <div id="actionsPointer_<%= Target.Id %>" class="borderBase pm-tasksItem-actionsPointer" style="background-image:url('<%=WebImageSupplier.GetAbsoluteWebPath("min_collapce.gif", ProductEntryPoint.ID)%>');">
                    &nbsp;
                </div>
                <img id="imgZub_<%= Target.Id %>" src="<%=WebImageSupplier.GetAbsoluteWebPath("zub_task.png", ProductEntryPoint.ID)%>" style="position:relative;left: 14px;"/>
            </div>
            <%} %>
        </td>
        </tr>
    </table>
    
</div>