<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MyNewTasksWidget.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Dashboard.MyNewTasksWidget" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<asp:Repeater ID="MyNewTasksRepeater" runat="server">
    <ItemTemplate>
        <div class="pm-dashboard-container">
            <div class="pm-dashboard-fadeParent pm-dashboard-bottomIndent">
                <div class="pm-dashboard-fade"></div> 
                <a class="linkHeaderLightMedium" title="<%# (Container.DataItem as Task).Title.HtmlEncode()%>"
                 href="tasks.aspx?prjID=<%#(Container.DataItem as Task).Project.ID%>&ID=<%#(Container.DataItem as Task).ID%>">
                    <%# (Container.DataItem as Task).Title.HtmlEncode()%>
                </a>
            </div>
            <div class="pm-dashboard-fadeParent pm-dashboard-bottomIndent">
                <div class="pm-dashboard-fade"></div>
                <span><%= ProjectResource.Project %>:</span>
                <a class="pm-dashboard-leftIndent-small" title="<%#(Container.DataItem as Task).Project.Title.HtmlEncode()%>"
                 href="projects.aspx?prjID=<%#(Container.DataItem as Task).Project.ID%>">
                    <%#(Container.DataItem as Task).Project.Title.HtmlEncode()%>
                </a>
            </div>
            <%# GetTaskDeadline((Container.DataItem as Task))%>
        </div>
    </ItemTemplate>
</asp:Repeater>     
<% if (HasData) %>
<% { %>
<a onclick="ASC.Projects.Reports.generateReportByUrl('<%=GetReportUri()%>')" style="cursor:pointer;">
    <%=ReportResource.GenerateReport%>
</a>
<% } %>