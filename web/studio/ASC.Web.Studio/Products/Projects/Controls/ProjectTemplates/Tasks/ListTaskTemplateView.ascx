<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListTaskTemplateView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.ProjectTemplates.Tasks.ListTaskTemplateView" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Projects.Core" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Src="TaskBlockTemplateView.ascx" TagPrefix="ctrl" TagName="TaskBlockTemplateView" %>
<%@ Register Src="TaskActionTemplateView.ascx" TagPrefix="ctrl" TagName="TaskActionTemplateView" %>
<%@ Register Src="MoveTaskTemplateView.ascx" TagPrefix="ctrl" TagName="MoveTaskView" %>

<script type="text/javascript" language="javascript">

jq(document).ready(function(){

ASC.Projects.Common.tooltip(".pm-task-title a","tooltip");

});

// jq(document).click(function(event)
// {

// ASC.Projects.Common.dropdownRegisterAutoHide(event, "#interval_switcher","#interval_dropdown");

// }); 


</script>

<style  type="text/css">


#filterBlock
{

    text-align:right;
    border-left: medium none; 
    border-right: medium none; 
    padding: 10px 0px;
    margin-bottom: 30px;
    
}

</style>

<div>
    <div class='clearFix borderBase'  id='filterBlock'>
		<asp:PlaceHolder ID="_tabsContainer" runat="server"></asp:PlaceHolder> 
    </div>
    <asp:Repeater runat="server" ID="_rptContent"  OnItemDataBound="rptContent_OnItemDataBound">
        <ItemTemplate>           
            <div class="pm-milestoneWithTasksBlock" "<%# (Eval("Milestone") is TemplateMilestone) ? "" : "style='padding-bottom:0px;'" %>"  id="milestoneWithTasksBlock_<%# Eval("Milestone") != null ? (Eval("Milestone") as TemplateMilestone).Id  : 0%>">
                  <div class='clearFix header' style="padding-bottom: 10px; ">
						<div style="float: right;margin-left: 5px;" class="clearFix">
							<%#  RenderAddTaskButton(Eval("Milestone") as TemplateMilestone)%>
						</div>
						<div class="clearFix">
							<%#  RenderInfoBlock(Eval("Milestone") as TemplateMilestone)%>
						</div>
                  </div> 
                <ctrl:TaskBlockTemplateView runat="server" ID="_taskBlockView" />
            </div>
        </ItemTemplate>      
    </asp:Repeater>  
    
    <% if (_rptContent.Items.Count == 0) %>    
    <% { %>
        <asp:PlaceHolder runat="server" ID="_phEmptyScreen" ></asp:PlaceHolder>
    <% } %>
    
    
    <%if(ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesUtil.CheckEditPermission(Template)) { %>    
    <ctrl:TaskActionTemplateView runat="server"  ID="_taskActionView"/>
    <ctrl:MoveTaskView runat="server"  ID="_moveTaskView"/>
    <%} %>
     
</div>


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
                    <%=   TaskResource.ExecuteGroupOperation%></a>
                <a class="grayLinkButton" href="javascript:void(0)" onclick="javascript: jq.unblockUI();" style="margin-left: 8px;">
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