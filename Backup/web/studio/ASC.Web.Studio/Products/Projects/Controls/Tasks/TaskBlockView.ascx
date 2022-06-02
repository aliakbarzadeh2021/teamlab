<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskBlockView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Tasks.TaskBlockView" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<%@ Register Src="OpenTaskBlockViewRow.ascx" TagPrefix="ctrl" TagName="OpenTaskBlockViewRow" %>
<%@ Register Src="ClosedTaskBlockViewRow.ascx" TagPrefix="ctrl" TagName="ClosedTaskBlockViewRow" %>

<% if (!OneList) %>
<%{%>
<table class="pm-tablebase pm-tasks-block" cellpadding="10" cellspacing="0" style="width: 100%">
    <thead>
        <tr>
            <td  class="borderBase"style="padding-left: 15px;">
                <%= TaskResource.TaskTitle%>                        
            </td>
            <% if (!IsAllMyTasks) %>
            <%{%>
            <td class="borderBase" style="width:150px;">
                <%= TaskResource.TaskResponsible%>
            </td>
            <%}%>
        </tr>
    </thead>
    <tbody>
    </tbody>
</table>
<%}%>

<div id="pm_openTaskContainer_<%=BlockMilestone!=null?BlockMilestone.ID : 0%>_<%=ProjectFat.Project.ID%>">
    <asp:Repeater runat="server" ID="rptContentOpen" OnItemDataBound="rptContentOpen_OnItemDataBound">
        <HeaderTemplate>
        </HeaderTemplate>
        <ItemTemplate>
            <ctrl:OpenTaskBlockViewRow runat="server" ID="_openTaskBlockViewRow" />
        </ItemTemplate>
        <FooterTemplate>
        </FooterTemplate>
    </asp:Repeater>
</div>

<div id="pm_closedTaskContainer_<%=BlockMilestone!=null?BlockMilestone.ID : 0%>_<%=ProjectFat.Project.ID%>">
    <asp:Repeater runat="server" ID="rptContentClosed" OnItemDataBound="rptContentClosed_OnItemDataBound">
        <HeaderTemplate>
        </HeaderTemplate>
        <ItemTemplate>
            <ctrl:ClosedTaskBlockViewRow runat="server" ID="_closedTaskBlockViewRow" />
        </ItemTemplate>
        <FooterTemplate>
        </FooterTemplate>
    </asp:Repeater>
</div>

<% if (!OneList) %>
<% { %>
<div style="padding-top:15px;">
    <% if (!ShowAllTasks) %>
    <% { %>
        <% if (IsVisible) %>
        <% { %>
        <a id="showLastCompletedTasks_<%=BlockMilestone!=null?BlockMilestone.ID:0%>_<%=ProjectFat.Project.ID%>" class="linkShowLastCompletedTasks" onclick="javascript:ASC.Projects.TaskActionPage.showLastCompletedTasks(<%=BlockMilestone!=null?BlockMilestone.ID:0%>,<%=ProjectFat.Project.ID%>)">
            <%= TaskResource.ShowMore%>
        </a>
        <% } %>
    <div id="hiddenContainer_<%=BlockMilestone!=null?BlockMilestone.ID:0%>_<%=ProjectFat.Project.ID%>">
        <asp:HiddenField ID="hfTaskList" runat="server" />
        <asp:HiddenField ID="hfLastTask" runat="server" />
        <asp:HiddenField ID="hfCountTask" runat="server" />
    </div>
    <% } %>
</div>
<% } %>







