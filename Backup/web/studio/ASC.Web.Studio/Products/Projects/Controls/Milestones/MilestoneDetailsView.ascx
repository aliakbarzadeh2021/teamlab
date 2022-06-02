<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MilestoneDetailsView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Milestones.MilestoneDetailsView" %>
<%@ Import Namespace="ASC.Projects.Core"%>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>
<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>
<%@ Register Src="~/Products/Projects/Controls/TimeSpends/TimeSpendActionView.ascx" TagPrefix="ctrl" TagName="TimeSpendActionView" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>


<script type="text/javascript" language="javascript">

jq(document).ready(function(){

ASC.Projects.Common.tooltip(".pm-task-title a","tooltip");

});


</script>

<div id="milestone-details-header" class=" pm-headerPanel-splitter">

    <div style="margin-left:90px;">
        <div style="padding-bottom:10px">
            <span class="textBigDescribe"><%=MilestoneResource.Status %>:</span>
            <span style="padding-left:5px;"><%=GetMilestoneStatus()%></span>
        </div>
        <div style="padding-bottom:10px">
            <span class="textBigDescribe"><%=MilestoneResource.Total %>:</span>
            <span style="padding-left:5px;"><%=GrammaticalHelperTotalTasksCount()%></span>
        </div>
        <div>
            <span class="textBigDescribe"><%=MilestoneResource.Progress%>:</span>
            <span style="padding-left:5px;"><%=GrammaticalHelperActiveTasksCount()%></span>
        </div>
    </div>

</div>

<%=GetActions() %>

<% if(countTasks!=0)%>
<% {%>
<div id='TasksBlock'>
    <div style="float: right;" class="clearFix">
        <%= RenderAddTaskButton()%>
    </div>
    <div class="headerBase pm-headerPanel-splitter" ><%=MilestoneResource.TasksInThisMilestone%></div>
    <div id="milestoneWithTasksBlock_<%=Milestone.ID %>">
        <asp:PlaceHolder ID="milestoneTasksContent" runat="server"></asp:PlaceHolder>
    </div>
</div>
<% }%>
<% else %>
<% {%>
<div id='emptyTasksBlock'>
    <asp:PlaceHolder runat="server" ID="taskEmptyContent" />
</div>
<% }%>

<div style="padding-top:10px;">
    <ascw:CommentsList ID="commentList" runat="server" BehaviorID="commentsList">
    </ascw:CommentsList>
</div>

<asp:PlaceHolder runat="server" ID="phAddTaskPanel" />
<asp:PlaceHolder runat="server" ID="phMoveTaskPanel" />

<div id="group_manager_popup" style="display: none;">
    <ascwc:container id="group_manager_container" runat="server">
        <header>    
            <%= TaskResource.GroupTaskConfirm%>
       </header>
        <body>
            <dl class="clearFix pm-flexible">
                <dt class="textBigDescribe">
                    <%= TaskResource.TaskList%>:</dt>
                <dd>
                    <ul>
                    </ul>
                </dd>
                <dt class="textBigDescribe">
                    <%=  ProjectsCommonResource.Action%>:</dt>
                <dd>
                </dd>
            </dl>
            <div class="pm-h-line">
                <!– –></div>
            <div class="pm-action-block">
                <a class="baseLinkButton">
                    <%=   TaskResource.ExecuteGroupOperation%></a> <span class="button-splitter"></span>
                <a class="grayLinkButton" href="javascript:void(0)" onclick="javascript: jq.unblockUI();">
                    <%= ProjectsCommonResource.Cancel%>
                </a>
            </div>
            <div class='pm-ajax-info-block' style="display: none;">
                <span style="font-style: italic; padding-left: 4px;" class="textMediumDescribe">
                    <%=  TaskResource.ExecutingGroupOperation%>
                </span>
                <br />
                <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
            </div>
        </body>
    </ascwc:container>
</div>

<% if (ASC.Web.Projects.Classes.Global.ModuleManager.IsVisible(ASC.Projects.Core.Domain.ModuleType.TimeTracking)) %>
<% { %>
<ctrl:TimeSpendActionView runat="server" ID="_timeSpendActionView" />
<% } %>