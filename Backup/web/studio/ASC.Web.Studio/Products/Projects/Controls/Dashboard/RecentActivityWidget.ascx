<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecentActivityWidget.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Dashboard.RecentActivityWidget" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Users.Activity" %>


<div class="pm-dashboard-fadeParent">
<table cellpadding="3">
    <asp:Repeater ID="LastActivityRepeater" runat="server">
    <ItemTemplate>
    <tr>
        <td valign="top">
            <div class="pm-dashboard-bottomIndent">
                <%#(Container.DataItem as UserActivity).Date.ToString(DateTimeExtension.DateFormatPattern)%>
            </div>
            <div class="pm-dashboard-bottomIndent">
                <%#(Container.DataItem as UserActivity).Date.ToString("HH:mm")%>
            </div>
        </td>
        <td valign="top">
                    <div class="pm-dashboard-bottomIndent" style="white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <%#EntityType(Container.DataItem as UserActivity)%>: <%#(Container.DataItem as UserActivity).ActionText%>
                    </div>
                             
                    <div class="pm-dashboard-bottomIndent" style="white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <a href="<%# (Container.DataItem as UserActivity).URL%>" title="<%#(Container.DataItem as UserActivity).Title.HtmlEncode()%>">
                            <%#(Container.DataItem as UserActivity).Title.HtmlEncode()%>
                        </a>
                    </div>
                    
                    <div class="pm-dashboard-bottomIndent" style="white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <%#EntityParentContainers(Container.DataItem as UserActivity)%>
                    </div>
                
                    <div class="pm-dashboard-bottomIndent" style="overflow:hidden;white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <span><%=MessageResource.From %>:</span>
                        <span class="pm-dashboard-leftIndent-small">
                            <%#ASC.Core.Users.StudioUserInfoExtension.RenderProfileLink(ASC.Core.CoreContext.UserManager.GetUsers((Container.DataItem as UserActivity).UserID), ProductEntryPoint.ID)%>
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