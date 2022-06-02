<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectActivity.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Projects.ProjectActivity" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<link href="<%= ASC.Web.Projects.Classes.PathProvider.GetFileStaticRelativePath("common.css") %>"
    type="text/css" rel="stylesheet" />

<script type="text/javascript">
    function generateReportByUrl(url)
    {
        open(url, "displayReportWindow", "status=yes,toolbar=yes,menubar=yes,scrollbars=yes");
    }
</script>

<div style="padding-bottom: 15px;">
    <a onclick="generateReportByUrl('<%=GetTasksReportUri()%>')" style="float: right;"
        class="grayLinkButton">
        <%= ReportResource.ShowUserTasksReport%>
    </a><span class="headerBase">
        <%= ProjectResource.Projects %></span>
</div>
<table class="pm-tablebase" cellpadding="14" cellspacing="0">
    <tbody>
        <asp:Repeater ID="ProjectsRepeater" runat="server">
            <ItemTemplate>
                <tr class=" <%# Container.ItemIndex%2==0 ? "tintMedium" : "tintLight" %>">
                    <td width="40%" class="borderBase">
                        <a href='products/projects/projects.aspx?prjID=<%# Eval("ProjectID") %>'>
                            <%# ((string)Eval("ProjectTitle")).HtmlEncode() %>
                        </a>
                    </td>
                    <td class="borderBase">
                        <a href='products/projects/tasks.aspx?prjID=<%# Eval("ProjectID") %>&action=3&userID=<%=userID %>'>
                            <%# Eval("OpenedTasksCount")%>
                            <%# GetOpenedTasksString((int)Eval("OpenedTasksCount"))%>
                        </a>
                    </td>
                    <td class="borderBase">
                        <a href='products/projects/tasks.aspx?prjID=<%# Eval("ProjectID") %>&action=3&userID=<%=userID %>&view=all'>
                            <%# Eval("ClosedTasksCount")%>
                            <%# GetClosedTasksString((int)Eval("ClosedTasksCount"))%>
                        </a>
                    </td>
                    <td class="borderBase">
                        <a href='products/projects/history.aspx?prjID=<%# Eval("ProjectID") %>&<%=HistoryRangeParams %>&uid=<%=userID %>&module=0&page=1'>
                            <%# Eval("ActivityCount")%>
                            <%# GetActivitiesString((int)Eval("ActivityCount"))%>
                    </td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>
    </tbody>
</table>
<div style="<%= ProjectsRepeater.Items.Count==0? "display:block;": "display:none;"%>">
    <%=ProjectResource.NotInvolvedInAnyProject%></div>
<div class="headerBase" style="padding-bottom: 15px; padding-top: 20px;">
    <a onclick="generateReportByUrl('<%=GetActivityReportUri()%>')" style="float: right;" class="grayLinkButton">
        <%= ReportResource.ShowUserActivityReport%>
    </a>
    <span class="headerBase">
        <%=ProjectsCommonResource.RecentActivity%>
    </span>
</div>    
<table class="pm-tablebase" cellpadding="14" cellspacing="0">
    <tbody>
        <asp:Repeater ID="LastActivityRepeater" runat="server">
            <ItemTemplate>
                <tr class=" <%# Container.ItemIndex%2==0 ? "tintMedium" : "tintLight" %>">
                    <td class="borderBase">
                        <%# Eval("DateString") %>
                    </td>
                    <td class="borderBase textBigDescribe">
                        <%# Eval("TimeString") %>
                    </td>
                    <td class="borderBase">
                        <%# Eval("EntityPlate") %>
                    </td>
                    <td class="borderBase" style="width: 100%">
                        <%# Eval("EntityType") %>
                        <span class="splitter"></span>
                        <%# Eval("EntityParentContainers")%>
                    </td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>
    </tbody>
</table>
<div style="<%= LastActivityRepeater.Items.Count==0? "display:block": "display:none"%>">
    <%=ProjectResource.NoActivity%></div>
