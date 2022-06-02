<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master"
    CodeBehind="reports.aspx.cs" Inherits="ASC.Web.Projects.Reports" %>

<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"
    TagPrefix="ascwc" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascctl" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<asp:Content ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent" runat="server">

    <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("reports.js") %>"></script>
    <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("sorttable.js") %>"></script>

    <link href="<%= PathProvider.GetFileStaticRelativePath("projects.css") %>" type="text/css" rel="stylesheet" />
    <link media="screen" href="<%= PathProvider.GetFileStaticRelativePath("reports.css") %>" type="text/css" rel="stylesheet" />
    <link media="print" href="<%= PathProvider.GetFileStaticRelativePath("reportsPrint.css") %>" type="text/css" rel="stylesheet" />
    
    <% if (!IsGenerate) %>
    <% { %>

    <script>
        jq(function()
        {
            jq("#rbl_"+<%=reportType%>).attr('checked','checked');
            jq("#cbxSaveAsTemplate").removeAttr("checked");
        });
        
        jq(document).ready(function()
        {
            ASC.Projects.Common.changeImage(".reportTypeRow img","<%=GetInfoOnImageUrl()%>","<%=GetInfoOffImageUrl()%>");
        });
        
        jq(document).click(function(event) 
        {
 
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher0","#reportInfo0");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher1","#reportInfo1");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher2","#reportInfo2");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher3","#reportInfo3");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher4","#reportInfo4");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher5","#reportInfo5");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher6","#reportInfo6");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher7","#reportInfo7");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher8","#reportInfo8");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher9","#reportInfo9");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher10","#reportInfo10");
            ASC.Projects.Common.dropdownRegisterAutoHide(event, "#reportInfoSwitcher11","#reportInfo11");
          
        });    
    </script>

    <% } %>
    <% else %>
    <% { %>

    <script>
        jq(document).ready(function() {
            if (jq("table.sortable") != null && jq("table.sortable tbody").children().length > 0) {
                var rows = jq("#result tbody").children();
                for (var j = 0; j < rows.length; j++) {
                    jq(rows[j]).removeClass("tintLight").removeClass("tintMedium");
                    if (j % 2 == 0) {
                        jq(rows[j]).addClass("tintMedium");
                    }
                    else {
                        jq(rows[j]).addClass("tintLight");
                    }
                }
            }
            jq("#studio_onlineUsersBlock").remove();
            jq("#studioFooter").remove();
            jq("div.studioTopNavigationPanel").remove();
            jq("div.infoPanel").remove();
            jq("#studioPageContent").css("padding-bottom","0");
        });
    </script>

    <% } %>
</asp:Content>
<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
    <% if (!IsGenerate) %>
    <% { %>
    <% if (HasTemplates) %>
    <% { %>
    <div class="pm-headerPanel-splitter">
        <div class="headerBase pm-headerPanel-splitter">
            <%= ReportResource.UseTemplate%>:</div>
        <select id="Templates" style="width: 400px;" class="comboBox">
            <%=InitTemplatesDdl()%>
        </select>
        <span class="splitter"></span><a class="baseLinkButton" style="margin-bottom: 3px;"
            onclick="ASC.Projects.Reports.generateReportByTemplateInNewWindow()">
            <%= ReportResource.GenerateFromTemplate%></a>
    </div>
    <% } %>
    <div class="headerBase pm-headerPanel-splitter">
        <%= ReportResource.CreateNewReport%>:
    </div>
    <table width="100%" cellpadding="0" cellspacing="0" class="pm-headerPanel-splitter">
        <tr valign="top">
            <td width="33%">
                <div class="pm-report-filtersHeader headerBase" style="white-space: nowrap">
                    <%= ReportResource.ReportProblemTracking_Title%>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_0" value="0" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_0" style="margin-right: 5px">
                        <%= ReportResource.ReportLateMilestones_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher0"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo0');" />
                    <div id="reportInfo0" class="reportInfo">
                        <%= GetReportDescription(0) %></div>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_1" value="1" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_1" style="margin-right: 5px">
                        <%= ReportResource.ReportUpcomingMilestones_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher1"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo1');" />
                    <div id="reportInfo1" class="reportInfo">
                        <%= GetReportDescription(1) %></div>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_2" value="2" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_2" style="margin-right: 5px">
                        <%= ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<ReportResource>("ReportEmployeesWithoutActiveTasks_Title")%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher2"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo2');" />
                    <div id="reportInfo2" class="reportInfo">
                        <%= GetReportDescription(2) %></div>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_3" value="3" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_3" style="margin-right: 5px">
                        <%= ReportResource.ReportProjectsWithoutActiveMilestones_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher3"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo3');" />
                    <div id="reportInfo3" class="reportInfo">
                        <%= GetReportDescription(3) %></div>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_4" value="4" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_4" style="margin-right: 5px">
                        <%= ReportResource.ReportProjectsWithoutActiveTasks_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher4"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo4');" />
                    <div id="reportInfo4" class="reportInfo">
                        <%= GetReportDescription(4) %></div>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_11" value="11" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_11" style="margin-right: 5px">
                        <%= ReportResource.ReportLateTasks_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher11"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo11');" />
                    <div id="reportInfo11" class="reportInfo">
                        <%= GetReportDescription(11) %></div>
                </div>
            </td>
            <td width="33%">
                <div class="pm-report-filtersHeader headerBase">
                    <%= ReportResource.ReportStatistics_Title%>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_5" value="5" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_5" style="margin-right: 5px">
                        <%= ReportResource.ReportUserActivity_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher5"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo5');" />
                    <div id="reportInfo5" class="reportInfo">
                        <%= GetReportDescription(5) %></div>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_6" value="6" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_6" style="margin-right: 5px">
                        <%=ReportResource.ReportEmployment_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher6"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo6');" />
                    <div id="reportInfo6" class="reportInfo">
                        <%= GetReportDescription(6) %></div>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_7" value="7" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_7" style="margin-right: 5px">
                        <%= ReportResource.ReportProjectList_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher7"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo7');" />
                    <div id="reportInfo7" class="reportInfo">
                        <%= GetReportDescription(7) %></div>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_8" value="8" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_8" style="margin-right: 5px">
                        <%= ReportResource.ReportTimeSpend_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher8"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo8');" />
                    <div id="reportInfo8" class="reportInfo">
                        <%= GetReportDescription(8) %></div>
                </div>
            </td>
            <td width="33%">
                <div class="pm-report-filtersHeader headerBase">
                    <%= ReportResource.ReportDetailed_Title%>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_9" value="9" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_9" style="margin-right: 5px">
                        <%= ReportResource.ReportTaskList_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher9"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo9');" />
                    <div id="reportInfo9" class="reportInfo">
                        <%= GetReportDescription(9) %></div>
                </div>
                <div class="reportTypeRow" style="white-space: nowrap">
                    <input name="rbl" id="rbl_10" value="10" type="radio" onclick="ASC.Projects.Reports.changeReportType(this.value);" />
                    <label for="rbl_10" style="margin-right: 5px">
                        <%= ReportResource.ReportUserTasks_Title%></label>
                    <img title="<%= ReportResource.ViewInfo %>" src="<%=GetInfoOffImageUrl()%>" id="reportInfoSwitcher10"
                        onclick="ASC.Projects.Common.dropdownToggle(this,'reportInfo10');" />
                    <div id="reportInfo10" class="reportInfo">
                        <%= GetReportDescription(10) %></div>
                </div>
            </td>
        </tr>
    </table>
    <% } %>
    <asp:PlaceHolder ID="reportFiltersPh" runat="server"></asp:PlaceHolder>
    <% if (IsGenerate) %>
    <% { %>
    <% if (!TemplateNotFound) %>
    <% { %>
    <asp:Literal ID="reportFilter" runat="server"></asp:Literal>
    <div id="actionContent" class="pm-headerPanel-splitter">
    <a style="float: left;" href="reports.aspx?reportType=<%=reportType%>&<%=Filter.ToUri()%>"><%=ReportResource.ChangeFilterData%></a>
    <% if (HasData) %>
    <% { %>
    <div style="text-align: right; white-space: nowrap;">
        <img align="absmiddle" src='<%=GetCsvImageUrl()%>' alt='<%=ReportResource.ExportToCSV%>'
            title='<%=ReportResource.ExportToCSV%>' />
        <a style="cursor: pointer;" class="linkAction" onclick="ASC.Projects.Reports.exportToCsv();">
            <%=ReportResource.ExportToCSV%></a> <span class="splitter"></span>
        <img align="absmiddle" src='<%=GetPrintImageUrl()%>' alt='<%=ReportResource.PrintReport%>'
            title='<%=ReportResource.PrintReport%>' />
        <a style="cursor: pointer;" class="linkAction" onclick="ASC.Projects.Reports.printReport();">
            <%=ReportResource.PrintReport%></a>
    </div>
    <% } %>
    </div>
    <asp:Literal ID="reportResult" runat="server"></asp:Literal>
    <% } %>
    <div id="emptyScreenContent">
        <asp:PlaceHolder ID="emptyScreenControlPh" runat="server"></asp:PlaceHolder>
    </div>
    <% } %>
</asp:Content>
<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server">
    <ascwc:SideNavigator runat="server" ID="SideNavigatorPanel">
    </ascwc:SideNavigator>
</asp:Content>
