<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectTeamView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Projects.ProjectTeamView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Web.Studio.Controls.Common" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"
    TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>



<asp:Repeater runat="server" ID="_rptContent">
    <HeaderTemplate>
       <div class="pm-projectTeam-h-line"><!– –></div>
    </HeaderTemplate>
    <ItemTemplate>
        <div class="<%#IsNewUser((Guid)Eval("UserInfo.ID")) %> clearFix <%#IsOldUser((Guid)Eval("UserInfo.ID")) %> pm-projectTeam-userContainer">
            <div class="pm-projectTeam-userInfoContainer">
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td class="pm-projectTeam-userPhotoContainer">
                            <img src="<%# ASC.Core.Users.UserInfoExtension.GetSmallPhotoURL(((ASC.Core.Users.UserInfo)Eval("UserInfo")))%>" />
                        </td>
                        <td>
                            <a class="linkHeader" href="<%# ASC.Web.Studio.Utility.CommonLinkUtility.GetUserProfile(((ASC.Core.Users.UserInfo)Eval("UserInfo")).ID, ProductEntryPoint.ID)%>">
                                <%# ASC.Web.Core.Users.DisplayUserSettings.GetFullUserName(((ASC.Core.Users.UserInfo)Eval("UserInfo"))).HtmlEncode()%>
                            </a>
                            <div class="pm-projectTeam-userAppointmentContainer">
                                <%# ((ASC.Core.Users.UserInfo)Eval("UserInfo")).Title.HtmlEncode()%>
                            </div>
                        </td>
                    </tr>
                </table>
            </div>
            <div class="pm-projectTeam-userInfoContainer">
                <div style='padding-left: 10px;white-space:nowrap;<%#((ASC.Core.Users.UserInfo)Eval("UserInfo")).Email == null ? "display:none" : "" %>'>
                    <img style="margin-right: 4px;" border="0" align="absmiddle" src="<%= ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("mail_user.png") %>" />
                    <a href="mailto:<%#((ASC.Core.Users.UserInfo)Eval("UserInfo")).Email %>">
                        <%#  ((ASC.Core.Users.UserInfo)Eval("UserInfo")).Email%>
                    </a>
                </div>
                <%if(!TemplateMode){ %>
                <div style="padding-left: 10px;margin-top: 4px;">
                    <a href="<%# ASC.Web.Studio.Utility.CommonLinkUtility.GetUserDepartment(((ASC.Core.Users.UserInfo)Eval("UserInfo")).ID)  %>">
                        <%# ((ASC.Core.Users.UserInfo)Eval("UserInfo")).Department.HtmlEncode()%>
                    </a>
                </div>
                <%} else{%>&nbsp;<%} %>
            </div>            
            <div class="pm-projectTeam-userInfoContainer">
				<%if(!TemplateMode){ %>
                <div style="padding-left: 10px;">
                    <a style="cursor:pointer;" class="linkAction" onclick="ASC.Projects.Reports.generateReportByUrl('<%#GetReportUri(true,(Guid)Eval("UserInfo.ID"))%>')">
                        <%= ReportResource.ReportOpenTasks %>
                    </a>
                </div>
                <div style="margin-top: 4px;padding-left: 10px;">
                    <a style="cursor:pointer;" class="linkAction" onclick="ASC.Projects.Reports.generateReportByUrl('<%#GetReportUri(false,(Guid)Eval("UserInfo.ID"))%>')">
                        <%= ReportResource.ReportClosedTasks %>
                    </a>
                </div>
                <%} else{%>
                <a href="<%# ASC.Web.Studio.Utility.CommonLinkUtility.GetUserDepartment(((ASC.Core.Users.UserInfo)Eval("UserInfo")).ID)  %>">
                    <%# ((ASC.Core.Users.UserInfo)Eval("UserInfo")).Department.HtmlEncode()%>
                </a>
                <%} %>
            </div>
        </div>
        <div class="pm-projectTeam-h-line <%#IsOldUser((Guid)Eval("UserInfo.ID")) %>"><!– –></div>              
    </ItemTemplate>
</asp:Repeater>
