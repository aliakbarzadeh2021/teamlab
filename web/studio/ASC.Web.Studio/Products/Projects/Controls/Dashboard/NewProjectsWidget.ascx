<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewProjectsWidget.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Dashboard.NewProjectsWidget" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %> 


<asp:Repeater ID="NewProjectsRepeater" runat="server">

    <ItemTemplate>
        <div class="pm-dashboard-container">
        <div class="pm-dashboard-bottomIndent">
            <div class="pm-dashboard-fadeParent">
                <div class="pm-dashboard-fade"></div>
                <%# IsPrivateProject((Container.DataItem as Project).Private)%> 
                <a class="linkHeaderLightMedium" title="<%# (Container.DataItem as Project).Title.HtmlEncode()%>"
                 href="projects.aspx?prjID=<%#(Container.DataItem as Project).ID%>">
                    <%# (Container.DataItem as Project).Title.HtmlEncode()%>
                </a>
            </div>
        </div>
            <div  style="overflow:hidden;white-space:nowrap;">
                <span><%=ProjectResource.ProjectLeader %>:</span>
                <span class="pm-dashboard-leftIndent-small"><%#GetProjectLeader(Container.DataItem as Project)%></span>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>
