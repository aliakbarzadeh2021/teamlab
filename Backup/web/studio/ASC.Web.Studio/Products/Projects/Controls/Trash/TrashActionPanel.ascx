<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TrashActionPanel.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Trash.TrashActionPanel" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>


<div id="group_manager_popup" style="display: none;">
    <ascwc:container id="CommonContainer" runat="server">
        <header>    
            <%= TaskResource.GroupTaskConfirm%>
       </header>
        <body>
            <dl class="clearFix pm-flexible">
                <dt class="textBigDescribe"><%= TaskResource.TaskList%>:</dt>
                <dd><div id="scroll"><ul></ul></div></dd>
                <dt id="1_1" class="textBigDescribe"><%= TaskResource.AssociateToMilestone%>:</dt>
                <dd id="1_2"><%= MilestoneSelect()%></dd>
                <dt id="2_1" class="textBigDescribe"><%=  ProjectsCommonResource.Action%>:</dt>
                <dd id="2_2"></dd>
            </dl>
            <div class="pm-h-line"><!– –></div>
            <div  class="pm-action-block">
                <a class="baseLinkButton">
                    <%= TaskResource.ExecuteGroupOperation%></a> <span class="button-splitter"></span>
                <a class="grayLinkButton" href="javascript:void(0)" onclick="javascript: jq.unblockUI();">
                    <%= ProjectsCommonResource.Cancel%>
                </a>
            </div>
            <div class='pm-ajax-info-block' style="display:none;">
              <span class="textMediumDescribe"><%= TaskResource.ExecutingGroupOperation%> </span><br />
              <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")%>" />
            </div>
        </body>
    </ascwc:container>
</div>