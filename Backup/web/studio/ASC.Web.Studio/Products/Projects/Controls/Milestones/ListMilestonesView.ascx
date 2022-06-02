<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListMilestonesView.ascx.cs" Inherits="ASC.Web.Projects.Controls.Milestones.ListMilestonesView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>


<style>
.pm-tablebase thead tr td
{
	padding:5px 15px;
	border-left:medium none!important;
	border-right:medium none!important;
}
</style>

<div>
    <div class="pm-headerPanel-splitter" id="<%=status %>MilestonesHeader">
        <img id="slide<%=status %>Milestones" style="cursor: pointer;" onclick="ASC.Projects.Milestones.slide<%=status %>Milestones('<%=GetFirstImageUrl() %>','<%=GetLastImageUrl() %>');" src="<%=GetFirstImageUrl() %>" title="<%=ProjectsCommonResource.Collapse %>" alt="<%=ProjectsCommonResource.Collapse %>"/>
        <span class="headerBase" style="cursor: pointer;" onclick="ASC.Projects.Milestones.slide<%=status %>Milestones('<%=GetFirstImageUrl() %>','<%=GetLastImageUrl() %>');"><%=GetTitle() %></span>
    </div>
    <div id="<%=status %>Milestones" class="pm-headerPanel-splitter">
    <table class="pm-tablebase" cellpadding="15" cellspacing="0">
        <thead>
            <tr>
                <td class="borderBase" width="24px"></td>
                <td class="borderBase" width="80px" style="padding-left:0px;"></td>
                <td class="borderBase"><%=MilestoneResource.MilestoneTitle%></td>
                <td class="borderBase" width="50px" style="text-align:center;"><%=TaskResource.Tasks%></td>
                <td class="borderBase" width="110px" style="text-align:center;">
                    <% if (status == "Active") %>
                    <% { %>
                    <%= ReportResource.ActiveTasks %>
                    <% } %>
                </td>
                
            </tr>
        </thead>
        <tbody>
            <asp:Repeater ID="MilestonesRepeater" runat="server">
                <ItemTemplate> 
                    <tr>
                        <td class="borderBase">
                            <%# RenderDeadlineImage(Container.DataItem as Milestone)%>
                        </td>
                        <td class="borderBase" style="padding-left:0px;">
                            <%# RenderDeadline(Container.DataItem as Milestone)%>
                        </td>
                        <td class="borderBase">
                            <div style="padding-bottom: 5px; overflow: hidden; width: 320px;">
                                <%# GetCaption(Container.DataItem as Milestone)%>
                            </div>
                        </td>
                        <td class="borderBase" style="text-align:center;">
                            <div style="overflow: hidden; width: 50px;">
                                <%# GetTotalTasksCount((Container.DataItem as Milestone).ID)%>
                            </div>
                        </td>
                        <td class="borderBase" style="text-align:center;">
                            <% if (status == "Active") %>
                            <% { %>
                            <div style="overflow: hidden; width: 110px;">
                            <%# GetProgress((Container.DataItem as Milestone).ID)%>
                            </div>
                            <% } %>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </tbody>
    </table>
</div>
</div>