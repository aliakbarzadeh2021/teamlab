<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Page Language="C#" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master"
    AutoEventWireup="true" CodeBehind="MyTasks.aspx.cs" Inherits="ASC.Web.Projects.MyTasks" %>

<%@ Import Namespace="ASC.Web.Projects.Classes" %>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common" TagPrefix="ascwc" %>
<%@ Register Src="~/Products/Projects/Controls/TimeSpends/TimeSpendActionView.ascx" TagPrefix="ctrl" TagName="TimeSpendActionView" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<asp:Content ID="Content1" ContentPlaceHolderID="BTHeaderContent" runat="server">

    <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("mytasks.js") %>"></script>
    <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("timetracking.js") %>"></script>
    <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("todo.js") %>"></script>

    <link href="<%= PathProvider.GetFileStaticRelativePath("tasks.css") %>" type="text/css" rel="stylesheet" />
    <link href="<%= PathProvider.GetFileStaticRelativePath("myTasks.css") %>" type="text/css" rel="stylesheet" />
    <link href="<%= PathProvider.GetFileStaticRelativePath("timetracking.css")%>" type="text/css" rel="stylesheet" />


    <script type="text/javascript">
    jq(document).ready(function(){
        <% if (OneList)  %>
        <%{ %>
        ASC.Projects.MyTaskPage.isOneList = true;
        <%} %>
   
        if(jq("tr[id*='pmtask_']").length == 0)
        {
            jq("#pm_exists_tasks").hide();            
            jq("#pm_title_tasks_one_list").hide();
            jq("#pm_empty_tasks").show();
        }
        
        jq(".containerHeaderBlock div").css("float","left");
        jq("#pm_view_switcher").insertAfter(jq(".containerHeaderBlock div")[0]);

        ASC.Projects.MyTaskPage.coloringRows();
        ASC.Projects.Common.tooltip(".pm-task-title a","tooltip");
 
    });
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BTPageContentWithoutCommonContainer"
    runat="server">
</asp:Content>
<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">

    <div id="pm_view_filter" style="border-bottom: 1px solid #d1d1d1; padding: 0 0 10px 5px;" class="clearFix">
        <asp:PlaceHolder ID="_tabsContainer" runat="server"></asp:PlaceHolder>
    </div>

    <asp:Panel ID="pnlTableTitle" runat="server" Visible="false">
        <div id="pm_title_tasks_one_list" style="padding-top: 20px;">
            <table class="pm-tablebase pm-tasks-block" cellpadding="10" cellspacing="0" style="width: 100%;">
                <thead>
                    <tr>
                        <td class="borderBase" style="padding-left: 15px;">
                            <%= TaskResource.TaskTitle%>
                        </td>
                    </tr>
                </thead>
            </table>
        </div>
    </asp:Panel>
    <div id="pm_exists_tasks">
        <asp:PlaceHolder runat="server" ID="_content"></asp:PlaceHolder>

        <asp:Literal ID="myTasksContent" runat="server"></asp:Literal>
    </div>
    <div id="pm_empty_tasks" style="display: none">
        <asp:PlaceHolder ID="_empty" runat="server"></asp:PlaceHolder>
    </div>

    <asp:PlaceHolder ID="timeSpendPlaceHolder" runat="server"></asp:PlaceHolder>
</asp:Content>
<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server">
</asp:Content>
