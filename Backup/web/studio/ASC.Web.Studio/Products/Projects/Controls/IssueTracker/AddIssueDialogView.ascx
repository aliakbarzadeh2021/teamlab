<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddIssueDialogView.ascx.cs" Inherits="ASC.Web.Projects.Controls.IssueTracker.AddIssueDialogView" %>

<style type="text/css">
    .headerPanelSmall
    {
        margin-bottom: 5px;
    }
    .pm-headerPanelSmall-splitter
    {
        margin-bottom: 10px;
    }
</style>

<div id="addIssuePanel">

<div class="pm-headerPanelSmall-splitter">
    <div class="headerPanelSmall">
        <%= IssueTrackerResource.IssueTitle %>
    </div>
    <asp:TextBox runat="server" CssClass="textEdit" ID="tbTitle" Width="100%" />
</div>
<div class="pm-headerPanelSmall-splitter">
    <div class="headerPanelSmall">
        <%= IssueTrackerResource.DetectedInVersion %>
    </div>
    <asp:TextBox runat="server" CssClass="textEdit" ID="tbDetectedInVersion" Width="100%" />
</div>
<div class="pm-headerPanelSmall-splitter">
    <div class="headerPanelSmall">
        <%= IssueTrackerResource.Priority %>
    </div>
    <div id="priority_block">
        <label>
            <asp:RadioButton name="ddlPriority" ID="ddlPriorityImmediate"  Text="0" runat="server" />
            <%= ASC.Web.Core.Helpers.ResourceEnumConverter.ConvertToString(IssuePriority.Immediate) %>
        </label>
        <label>
            <asp:RadioButton name="ddlPriority" ID="ddlPriorityUrgent" Text="1" runat="server" />
            <%= ASC.Web.Core.Helpers.ResourceEnumConverter.ConvertToString(IssuePriority.Urgent) %>
        </label>
        <label>
            <asp:RadioButton name="ddlPriority" ID="ddlPriorityHigh" Text="2" runat="server" />
            <%= ASC.Web.Core.Helpers.ResourceEnumConverter.ConvertToString(IssuePriority.High) %>
        </label>
        <label>
            <asp:RadioButton name="ddlPriority" ID="ddlPriorityNormal" Text="3" runat="server" />
            <%= ASC.Web.Core.Helpers.ResourceEnumConverter.ConvertToString(IssuePriority.Normal) %>
        </label>
        <label>
            <asp:RadioButton name="ddlPriority" ID="ddlPriorityLow" Text="4" runat="server" />
            <%= ASC.Web.Core.Helpers.ResourceEnumConverter.ConvertToString(IssuePriority.Low) %>
        </label>
    </div>
</div>
<div class="pm-headerPanelSmall-splitter">
    <div class="headerPanelSmall">
        <%= IssueTrackerResource.Description %>
    </div>    
    <FCKeditorV2:FCKeditor ID="tbDescription" Height="400px" runat="server">
    </FCKeditorV2:FCKeditor>
</div>
<div class="pm-headerPanelSmall-splitter">
    <div class="headerPanelSmall">
        <%= IssueTrackerResource.AssignedOn %>
    </div>
    <asp:DropDownList runat="server" CssCLass="comboBox" Width="99%" ID="ddlParticipants">
    </asp:DropDownList>
</div>
<div class="pm-headerPanelSmall-splitter">
    <input type="checkbox" checked="checked" name='notify_assigned_on_issue' id='notify_assigned_on_issue' />
    <label for='notify_assigned_on_issue'>
        <%= ProjectsCommonResource.NotifyPeopleViaEmail%></label>
</div>

<div class="pm-headerPanelSmall-splitter">
    <div class="headerPanelSmall">
        <%= IssueTrackerResource.CorrectedInVersion %>
    </div>
    <asp:TextBox runat="server" CssClass="textEdit" ID="tbCorrectedInVersion" Width="100%" />
</div>
<div class="pm-h-line">
    <!– –>
</div>
<div class="pm-action-block">
    <a href="javascript:void(0)" onclick="ASC.Projects.IssueTrackerActionPage.saveOrUpdate(-1, '<%= tbDescription.ClientID %>')" class="baseLinkButton">
        <%= IssueTrackerResource.AddThisIssue %>
    </a>
    <span class="button-splitter"></span>
    <a class="grayLinkButton" href="javascript:void(0)" onclick="javascript: jq.unblockUI();">
        <%= ProjectsCommonResource.Cancel %>
    </a>
</div>
<div class='pm-ajax-info-block' style="display: none;">
    <span class="textMediumDescribe">
        <%= IssueTrackerResource.SavingIssue %></span><br />
    <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
</div>

</div>