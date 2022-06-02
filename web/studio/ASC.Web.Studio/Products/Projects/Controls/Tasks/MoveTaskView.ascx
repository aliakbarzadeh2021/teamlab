<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MoveTaskView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Tasks.MoveTaskView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<div id="moveTaskPanel" style="display: none;">
    <ascwc:container id="moveTaskContainer" runat="server">
        <header>    
            <%= TaskResource.MoveTaskToAnotherMilestone%>
        </header>
        <body>
            <div class="borderBase" style="padding:20px 30px 30px 20px;margin-bottom:20px;">
                <div class="textBigDescribe">
                    <%= TaskResource.Task %>
                </div>
                <div style="padding:10px 0px;overflow-x: hidden;">
                    <b id="moveTaskTitles"></b>
                </div>
                <div class="textBigDescribe" style="padding-bottom:10px;"><%= TaskResource.WillBeMovedToMilestone%>:</div>
                <div style="overflow-x: hidden;">
                    <asp:Repeater ID="rptMilestoneSelector" runat="server">
                        <ItemTemplate>
                            <div class='<%# Container.ItemIndex%2==0 ? "tintMedium" : "tintLight" %>' style="border-top: 1px solid #D1D1D1; padding: 5px;">
                                <input id="milestone_<%# Eval("ID")%>" type="radio" name="milestones" <%# Container.ItemIndex==0 ? "checked='checked'" : "" %> />
                                <label for="milestone_<%# Eval("ID")%>"><%# GetMilestoneTitle((Container.DataItem as Milestone))%></label>
                            </div>           
                        </ItemTemplate>
                    </asp:Repeater>
                    <div class="<%=CssClassForEmptyMilestone %>" style="border-top: 1px solid #D1D1D1;border-bottom: 1px solid #D1D1D1; padding: 5px;">
                        <input id="milestone_0" type="radio" name="milestones"/>
                        <label for="milestone_0"><%= TaskResource.None%></label>
                    </div>
                </div>
            </div>
            
            <div class="pm-action-block">
                <a href="javascript:void(0)" class="baseLinkButton">
                    <%= TaskResource.MoveToMilestone%>
                </a>
                <span class="button-splitter"></span>
                <a class="grayLinkButton" href="javascript:void(0)" onclick="javascript: jq.unblockUI();">
                    <%= ProjectsCommonResource.Cancel%>
                </a>
            </div>
            <div class='pm-ajax-info-block' style="display: none;">
                <span class="textMediumDescribe">
                    <%= TaskResource.ExecutingGroupOperation%>
                </span><br />
                <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
            </div>
        </body>
    </ascwc:container>
</div>