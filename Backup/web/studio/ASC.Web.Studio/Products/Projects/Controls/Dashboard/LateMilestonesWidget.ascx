<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LateMilestonesWidget.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Dashboard.LateMilestonesWidget" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Controls.Dashboard" %>

<div class="pm-dashboard-fadeParent">
    <table cellpadding="3">
        <asp:Repeater ID="MilestonesRepeater" runat="server">
            <ItemTemplate>
                <tr>
                    <td align="center">
                        <div class="pm-redText" style="font-family: Arial; font-size: 36px; font-weight: bold;">
                            <%# (Container.DataItem as WrapperLateMilestonesWidget).daysLate%>
                        </div>
                        <div class="pm-redText" style="margin-top: -5px;">
                            <%# GrammaticalHelperDays((Container.DataItem as WrapperLateMilestonesWidget).daysLate)%>
                        </div>
                    </td>
                    <td>
                        <div class="pm-dashboard-bottomIndent" style="padding-top: 5px;">
                            <div class="pm-dashboard-fade">
                            </div>
                            <%# IsRootMilestone((Container.DataItem as WrapperLateMilestonesWidget).isRoot)%>
                            <a class="linkHeaderLightMedium" style="white-space: nowrap;" title="<%#(Container.DataItem as WrapperLateMilestonesWidget).milestoneTitle.HtmlEncode()%>"
                                href="milestones.aspx?prjID=<%#(Container.DataItem as WrapperLateMilestonesWidget).projectID%>&ID=<%#(Container.DataItem as WrapperLateMilestonesWidget).milestoneID%>">
                                <%#(Container.DataItem as WrapperLateMilestonesWidget).milestoneTitle.HtmlEncode()%>
                            </a>
                        </div>
                        <div class="pm-dashboard-bottomIndent">
                            <div class="pm-dashboard-fade">
                            </div>
                            <span>
                            <%= ProjectResource.Project%>:
                        </span><a class="pm-dashboard-leftIndent-small" style="white-space: nowrap;" title="<%#(Container.DataItem as WrapperLateMilestonesWidget).projectTitle.HtmlEncode()%>"
                            href="projects.aspx?prjID=<%#(Container.DataItem as WrapperLateMilestonesWidget).projectID%>">
                            <%#(Container.DataItem as WrapperLateMilestonesWidget).projectTitle.HtmlEncode()%>
                        </a>
                        </div>
                        <div class="pm-dashboard-bottomIndent" style="overflow: hidden; white-space: nowrap;">
                            <div class="pm-dashboard-fade">
                            </div>
                            <span><%=ProjectResource.ProjectLeader%>:</span><span class="pm-dashboard-leftIndent-small">
                            <%#GetProjectLeader((Container.DataItem as WrapperLateMilestonesWidget).projectLeader)%>
                        </span>
                        </div>
                    </td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>
    </table>
<% if (HasData) %>
<% { %>
    <a onclick="ASC.Projects.Reports.generateReportByUrl('<%=GetReportUri()%>')" style="margin-left:5px;cursor:pointer;">
        <%=ReportResource.GenerateReport%>
    </a>
<% } %>    
</div>