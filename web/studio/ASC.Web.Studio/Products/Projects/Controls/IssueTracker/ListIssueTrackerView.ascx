<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Core.Helpers" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Controls.Reports" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListIssueTrackerView.ascx.cs" Inherits="ASC.Web.Projects.Controls.IssueTracker.ListIssueTrackerView" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Src="IssueItemView.ascx" TagPrefix="ctrl" TagName="IssueItemView" %>

<asp:Repeater ID="rptIssuesList" runat="server" OnItemDataBound="rptIssuesList_OnItemDataBound">
    <HeaderTemplate>
        <table class="pm-tablebase pm-issue-block" cellpadding="15" cellspacing="0" style="width: 100%">
            <thead>
                <tr>
                    <td class="borderBase pm-issue-status" style="white-space:nowrap; width:91px; padding-left: 25px;" >
                        <%= ProjectsCommonResource.Status%>
                    </td>
                    <td  class="borderBase" style="width: 90px">
                        <%= IssueTrackerResource.IssueID %>
                    </td>                    
                    <td  class="borderBase">
                        <%= IssueTrackerResource.IssueTitle%>
                    </td>
                   
                    <td class="borderBase" style="width:130px;">
                        <%= IssueTrackerResource.AssignedOn%>
                    </td>
                </tr>
            </thead>
            <tbody>            
            </tbody> 
      </table>
      <div id="pm_issueContainer"></div>
    </HeaderTemplate>
    <ItemTemplate>
        <ctrl:IssueItemView runat="server" ID="_issueItemView" />
    </ItemTemplate>
    <FooterTemplate>
    </FooterTemplate>
</asp:Repeater>

<div>
    <a class="baseLinkButton" href="issueTracker.aspx?prjID=<%= Project.ID %>&id=-1"><%= IssueTrackerResource.AddIssue %></a>
</div>