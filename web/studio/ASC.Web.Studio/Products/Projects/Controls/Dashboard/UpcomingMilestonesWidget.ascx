<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UpcomingMilestonesWidget.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Dashboard.UpcomingMilestonesWidget" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>


<div class="pm-dashboard-fadeParent">
<table cellpadding="3">
    <asp:Repeater ID="MilestonesRepeater" runat="server">
    <ItemTemplate>
    <tr>
        <td valign="top">
            <div class="pm-dashboard-bottomIndent">
                <%# GetMilestoneDate(Container.DataItem as Milestone)%>
            </div>
        </td>
        <td valign="top">
        
                    <div class="pm-dashboard-bottomIndent" style="white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <%# IsRootMilestone((Container.DataItem as Milestone).IsKey)%>
                        <a class="linkHeaderLightMedium" title="<%#(Container.DataItem as Milestone).Title.HtmlEncode()%>"
                         href="milestones.aspx?prjID=<%#(Container.DataItem as Milestone).Project.ID%>&ID=<%#(Container.DataItem as Milestone).ID%>">
                            <%#(Container.DataItem as Milestone).Title.HtmlEncode()%>
                        </a>
                    </div>
                             
                    <div class="pm-dashboard-bottomIndent" style="white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <span><%= ProjectResource.Project %>:</span>
                        <a class="pm-dashboard-leftIndent-small" title="<%#(Container.DataItem as Milestone).Project.Title.HtmlEncode()%>"
                         href="projects.aspx?prjID=<%#(Container.DataItem as Milestone).Project.ID%>">
                            <%#(Container.DataItem as Milestone).Project.Title.HtmlEncode()%>
                        </a>
                    </div>
                
                    <div class="pm-dashboard-bottomIndent" style="overflow:hidden;white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <span><%=ProjectResource.ProjectLeader %>:</span>
                        <span class="pm-dashboard-leftIndent-small"><%#GetProjectLeader(Container.DataItem as Milestone)%></span>
                    </div>
        </td>
    </tr>
    </ItemTemplate>
    </asp:Repeater> 
</table>
<% if (HasData) %>
<% { %>
    <a onclick="ASC.Projects.Reports.generateReportByUrl('<%=GetReportUri()%>')"  style="margin-left:5px;cursor:pointer;">
        <%=ReportResource.GenerateReport%>
    </a>
<% } %>
</div>