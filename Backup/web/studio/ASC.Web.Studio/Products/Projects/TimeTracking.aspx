<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="timeTracking.aspx.cs" Inherits="ASC.Web.Projects.TimeTracking" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common" TagPrefix="ascwc" %>  
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<%@ Register Src="Controls/TimeSpends/TimeSpendEditView.ascx" TagPrefix="ctrl" TagName="TimeSpendEditView" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascctl" %>  

<asp:Content ID="Content1" ContentPlaceHolderID="BTHeaderContent" runat="server">
    
    <link href="<%= PathProvider.GetFileStaticRelativePath("reports.css") %>" rel="stylesheet" type="text/css" />
    <link href="<%= PathProvider.GetFileStaticRelativePath("timetracking.css")%>" rel="stylesheet" type="text/css" />

    <script src="<%= PathProvider.GetFileStaticRelativePath("timetracking.js")%>" type="text/javascript"></script>
    <script src="<%= PathProvider.GetFileStaticRelativePath("reports.js") %>" type="text/javascript"></script>
    <style type="text/css">
    table.pm-tablebase-timeTracking tbody td
     {
        padding-top:10px;
        vertical-align:top;
    }
    </style>
    <% if (IsTimer) %>
    <% { %>
    <script>
        jq(document).ready(function() {
            jq("#studio_onlineUsersBlock").remove();
            jq("#studioFooter").remove();
            jq("div.studioTopNavigationPanel").remove();
            jq("div.infoPanel").remove();
            jq("div.containerHeaderBlock").remove();
            jq("#studioPageContent").css("padding-bottom","0");
            jq("div.containerBodyBlock").css("padding","0");
            jq("div.mainPageLayout").css("width","100%");
            jq("div.mainPageLayout div:first").remove();
            jq("div.mainContainerClass").css("border","medium none");
            jq("#studioContent").css("overflow","hidden");
            initSelectUserTasksByProject();
        });
    </script>
    <% } %>
</asp:Content>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">

    <% if (!IsTimer) %>
    <% { %>
    <div id="MainPageContainer" runat="server">
        
        <div id="mainContent" class="pm-headerPanel-splitter">
        
            
            <table width="100%" cellpadding="5" cellspacing="0" class="pm-tablebase">
                <thead>
                    <tr>
                        <td style="width: 100px;text-align:center;"><%= ProjectsCommonResource.Date %></td>
                        <td class="pm-ts-personColumn"><%=TaskResource.TaskResponsible %></td>
                        <td style="width: 40px;"><%= ProjectsCommonResource.HoursCount %></td>
                        <td class="pm-ts-noteColumn"><%=ProjectResource.ProjectDescription %></td>
                        <td class="pm-ts-actionsColumn"></td>
                    </tr>
                </thead>
                      
            </table>        
            
            <div id="quickAddTimeSpendsList">
                
            </div>
            
            <div id="timeSpendsList">
                <asp:Repeater id="timeSpendRpt" runat="server">
                    
                    <ItemTemplate>
                        <div id="timeSpendRecord<%# (Container.DataItem as TimeSpendVM).TimeSpend.ID%>" class="<%#Container.ItemIndex%2==0 ? "tintMedium" : "tintLight" %>" onmouseover="ASC.Projects.TimeSpendActionPage.showActions(<%# (Container.DataItem as TimeSpendVM).TimeSpend.ID%>);" onmouseout="ASC.Projects.TimeSpendActionPage.hideActions(<%# (Container.DataItem as TimeSpendVM).TimeSpend.ID%>);">
                            <table width="100%" cellpadding="5" cellspacing="0" class="pm-tablebase pm-tablebase-timeTracking">

                            <tr height="36px">
                                <td class="borderBase pm-ts-dateColumn" style="border-top:0px !important">
                                    <span id="date_ts<%# (Container.DataItem as TimeSpendVM).TimeSpend.ID%>" class="pm-grayText">
                                        <%# (Container.DataItem as TimeSpendVM).TimeSpend.Date.ToString(DateTimeExtension.DateFormatPattern)%>
                                    </span>
                                </td>
                                <td class="borderBase pm-ts-personColumn" style="border-top:0px !important">
                                    <span id="person_ts<%# (Container.DataItem as TimeSpendVM).TimeSpend.ID%>">
                                        <%# ASC.Core.Users.StudioUserInfoExtension.RenderProfileLink(ASC.Core.CoreContext.UserManager.GetUsers((Container.DataItem as TimeSpendVM).TimeSpend.Person), ASC.Web.Projects.Configuration.ProductEntryPoint.ID)%>
                                    </span>
                                </td>
                                <td class="borderBase pm-ts-hoursColumn" style="border-top:0px !important">
                                    <strong id="hours_ts<%# (Container.DataItem as TimeSpendVM).TimeSpend.ID%>">
                                        <%#  (Container.DataItem as TimeSpendVM).TimeSpend.Hours.ToString("N2")%>
                                    </strong>
                                </td>
                                <td class="borderBase pm-ts-noteColumn" title="<%# GetTitle(Container.DataItem as TimeSpendVM) %>" style="border-top:0px !important">
                                    <div id="note_ts<%# (Container.DataItem as TimeSpendVM).TimeSpend.ID%>" style="width:340px;overflow:hidden;">
                                        <%#  GetNote(Container.DataItem as TimeSpendVM) %>
                                    </div>
                                </td>
                                <td class="borderBase pm-ts-actionsColumn" style="border-top:0px !important">
                                    <div id="ts_actions<%# (Container.DataItem as TimeSpendVM).TimeSpend.ID%>" style="display:none;">
                                        <%#  GetAction((Container.DataItem as TimeSpendVM).TimeSpend)%>
                                    </div>
                                </td>
                            </tr>

                            </table>
                        </div>
                    </ItemTemplate>
                    
                </asp:Repeater>
            </div>
            
        </div>
        
        
        <div style="text-align: right; margin:0 407px 10px 0;<%=VisibleTotalHoursCountOnPage%>">
            <span><%= ProjectsCommonResource.TotalOnPage%>:</span>
            <span class="splitter"></span>
            <strong id="totalHoursCountOnPage"><%=TotalHoursCountOnPage.ToString("N2")%></strong>
        </div>
        
        <div style="text-align: right; margin:0 407px 20px 0;">
            <span><%= ProjectsCommonResource.Total%>:</span>
            <span class="splitter"></span>
            <strong id="totalHoursCount"><%=TotalHoursCount.ToString("N2")%></strong>
        </div>   
        
        <asp:PlaceHolder ID="phContent" runat="server"></asp:PlaceHolder>
        
        <asp:HiddenField ID="hiddenCssClass" runat="server"/>
        
        <ctrl:TimeSpendEditView runat="server" ID="_timeSpendEditView" />

    </div>
    <div id="EmptyScreenContainer" runat="server"></div>
    <% } %>
    <% else %>
    <% { %>
    <asp:PlaceHolder ID="_phTimeSpendTimer" runat="server"></asp:PlaceHolder>
    <% } %>
    
</asp:Content>

<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server" >

    <ascwc:SideActions runat="server" ID="SideActionsPanel">
    </ascwc:SideActions>
    <ascwc:SideNavigator runat="server" ID="SideNavigatorPanel">
    </ascwc:SideNavigator>
    

</asp:Content>