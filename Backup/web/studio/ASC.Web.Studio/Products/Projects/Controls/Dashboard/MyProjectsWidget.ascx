<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MyProjectsWidget.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Dashboard.MyProjectsWidget" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>



<asp:Repeater ID="MyProjectsRepeater" runat="server">

    <ItemTemplate>
        <div class="pm-dashboard-container">
        
            <div class="pm-dashboard-bottomIndent">
                <div class="pm-dashboard-fadeParent" style="max-width: 200px;float: left;margin-right: 10px;">
                    <%# IsPrivateProject((Container.DataItem as ProjectVM).Project.Private) %>
                    <a class="linkHeaderLightMedium" title="<%# (Container.DataItem as ProjectVM).Project.Title.HtmlEncode() %>"
                     href="projects.aspx?prjID=<%#(Container.DataItem as ProjectVM).Project.ID%>">
                        <%# ASC.Web.Controls.HtmlUtility.GetText((Container.DataItem as ProjectVM).Project.Title, 25).HtmlEncode()%>
                    </a>
                </div>
                <div>
                    <%= ProjectsCommonResource.With %>
                    <span class="pm-dashboard-leftIndent-small">
                        <%# ActiveTasksInProject(Container.DataItem as ProjectVM)%>    
                    </span>
                </div>
            </div>
            
            <div style="clear:left;"></div>
            
            
        </div>
    </ItemTemplate>
</asp:Repeater> 
<% if (HasData) %>
<% { %>
<a onclick="ASC.Projects.Reports.generateReportByUrl('<%=GetReportUri()%>')" style="cursor:pointer;">
    <%=ReportResource.GenerateReport%>
</a>
<% } %>